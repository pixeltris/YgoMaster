using System;
using System.Collections.Generic;
using System.IO;

// Implements the in-game gem shop ("Purchase Gems"), so gems can be obtained through the game's
// own store UI instead of by hand editing Player.json. Bundles are defined in Data/Billing.json.
//
// Act order for a single purchase:
//   Billing.product_list                  populates the store (Billing.json verbatim)
//   Billing.Steam.in_complete_item_check   client checks for interrupted purchases
//   Billing.reservation                    -> ticket
//   Billing.purchase                       -> ticket
//   Billing.add_purchased_item             -> grants the items, refreshes the store
//
// REQUIRES the YgomSystem.Billing.Billing_Steam hook in YgoMasterClient/Program.cs. The client
// normally hands off to the Steam overlay between reservation and purchase and waits for a
// MicroTxnAuthorizationResponse_t callback, which never arrives offline because SteamAPI is
// stubbed. Without that hook the store hangs on a loading spinner and these acts past
// reservation are never sent.
//
// The "ticket" is a base64 json blob the server mints and the client echoes back on subsequent
// acts. Nothing validates it here; it exists because the client expects the round trip.

namespace YgoMaster
{
    partial class GameServer
    {
        // Reloaded per call so Billing.json can be edited without restarting the server.
        // NOTE: DeserializeStripped (not Deserialize) so // comments in the file are tolerated.
        Dictionary<string, object> LoadBillingData()
        {
            string path = Path.Combine(dataDirectory, "Billing.json");
            if (!File.Exists(path))
            {
                Utils.LogWarning("Billing.json not found at " + path);
                return null;
            }
            return MiniJSON.Json.DeserializeStripped(File.ReadAllText(path)) as Dictionary<string, object>;
        }

        Dictionary<string, object> GetBillingProduct(string productKey)
        {
            if (string.IsNullOrEmpty(productKey))
            {
                return null;
            }
            Dictionary<string, object> billing = LoadBillingData();
            Dictionary<string, object> gemShop = Utils.GetDictionary(billing, "GemShop");
            Dictionary<string, object> products = Utils.GetDictionary(gemShop, "Products");
            if (products == null)
            {
                return null;
            }
            return Utils.GetDictionary(products, productKey);
        }

        // The "v" ticket (base64 json) is the one field present in every billing act, and it
        // carries "mid" - the product id. This is the only reliable way to identify the
        // product on purchase / add_purchased_item, which don't send merchID.
        string GetProductKeyFromTicket(GameServerWebRequest request)
        {
            string ticket = Utils.GetValue<string>(request.ActParams, "v");
            if (string.IsNullOrEmpty(ticket))
            {
                return null;
            }
            try
            {
                string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ticket));
                Dictionary<string, object> values = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
                object mid;
                if (values != null && values.TryGetValue("mid", out mid) && mid != null)
                {
                    return mid.ToString();
                }
            }
            catch (Exception e)
            {
                Utils.LogWarning("Failed to decode billing ticket: " + e.Message);
            }
            return null;
        }

        // Billing.purchase sends "receipt", which is the store product_id (not the key).
        string GetProductKeyFromReceipt(GameServerWebRequest request)
        {
            string receipt = Utils.GetValue<string>(request.ActParams, "receipt");
            if (string.IsNullOrEmpty(receipt))
            {
                return null;
            }
            Dictionary<string, object> products = Utils.GetDictionary(
                Utils.GetDictionary(LoadBillingData(), "GemShop"), "Products");
            if (products == null)
            {
                return null;
            }
            foreach (KeyValuePair<string, object> entry in products)
            {
                Dictionary<string, object> product = entry.Value as Dictionary<string, object>;
                if (product != null && Utils.GetValue<string>(product, "product_id") == receipt)
                {
                    return entry.Key;
                }
            }
            return null;
        }

        // Each billing act identifies the bundle differently:
        //   Billing.reservation        -> "merchID" (the key into Billing.json Products)
        //   Billing.purchase           -> "v" ticket, plus "receipt" (the store product_id)
        //   Billing.add_purchased_item -> "v" ticket only
        // The ticket is the only field common to all three, so it's the main path.
        string GetRequestedProductKey(GameServerWebRequest request)
        {
            foreach (string key in new string[] { "merchID", "shop_paid_id", "mid", "product_id", "id" })
            {
                object val;
                if (request.ActParams != null && request.ActParams.TryGetValue(key, out val) && val != null)
                {
                    return val.ToString();
                }
            }

            string fromTicket = GetProductKeyFromTicket(request);
            if (!string.IsNullOrEmpty(fromTicket))
            {
                return fromTicket;
            }

            string fromReceipt = GetProductKeyFromReceipt(request);
            if (string.IsNullOrEmpty(fromReceipt))
            {
                // Dump the params so a client change to these field names is diagnosable.
                Utils.LogWarning(request.ActName + " couldn't resolve a product. params: " +
                    MiniJSON.Json.Serialize(request.ActParams));
            }
            return fromReceipt;
        }

        // Nonces from the real service take the form "<number>:<16 hex chars>". The client isn't
        // known to parse it, but the format is matched in case it ever does.
        string CreateBillingNonce(GameServerWebRequest request)
        {
            return request.Player.Code.ToString() + ":" + Guid.NewGuid().ToString("N").Substring(0, 16);
        }

        void Act_BillingProductList(GameServerWebRequest request)
        {
            Dictionary<string, object> billing = LoadBillingData();
            if (billing == null)
            {
                return;
            }
            request.Response = billing;
            request.Remove("GemShop.Products", "GemShop.BuyResult", "GemShop.ConfirmReg");
        }

        void Act_Billing_Steam_in_complete_item_check(GameServerWebRequest request)
        {
            request.Response["InCompleteList"] = new List<object>();
        }

        // The real service emits "mid" as an INTEGER here ({"mid":1,...}) but as a STRING in the
        // purchase ticket ({"mid":"1",...}). The inconsistency is intentional on our side too -
        // don't normalise the two to match each other.
        void Act_BillingReservation(GameServerWebRequest request)
        {
            string productKey = GetRequestedProductKey(request);
            Dictionary<string, object> product = GetBillingProduct(productKey);
            if (product == null)
            {
                Utils.LogWarning("Billing.reservation: unknown product '" + productKey + "'");
                return;
            }

            int mid;
            int.TryParse(productKey, out mid);

            WriteBillingTicket(request, new Dictionary<string, object>()
            {
                { "mid", mid },
                { "pid", Utils.GetValue<string>(product, "product_id") },
                { "nonce", CreateBillingNonce(request) },
            });
        }

        void Act_BillingPurchase(GameServerWebRequest request)
        {
            string productKey = GetRequestedProductKey(request);
            Dictionary<string, object> product = GetBillingProduct(productKey);
            if (product == null)
            {
                Utils.LogWarning("Billing.purchase: unknown product '" + productKey + "'");
                return;
            }

            // Prefer what the client sent, fall back to Billing.json.
            string price = Utils.GetValue<string>(request.ActParams, "price");
            string currency = Utils.GetValue<string>(request.ActParams, "currency");

            WriteBillingTicket(request, new Dictionary<string, object>()
            {
                { "mid", productKey },
                { "pid", Utils.GetValue<string>(product, "product_id") },
                { "nonce", CreateBillingNonce(request) },
                { "price", string.IsNullOrEmpty(price) ? Utils.GetValue<string>(product, "price") : price },
                { "currency", string.IsNullOrEmpty(currency) ? Utils.GetValue<string>(product, "currency") : currency },
                { "purchased_item_target_id", Utils.CreateBillingId() },
                { "is_restore", false },
                { "is_create_reservation", false },
            });
        }

        void WriteBillingTicket(GameServerWebRequest request, Dictionary<string, object> ticket)
        {
            string encoded = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(MiniJSON.Json.Serialize(ticket)));

            request.Response["Persistence"] = new Dictionary<string, object>()
            {
                { "System", new Dictionary<string, object>() {
                    { "ticket", new Dictionary<string, object>() {
                        { "v", encoded }
                    }}
                }}
            };
        }

        void Act_Billing_add_purchased_item(GameServerWebRequest request)
        {
            string productKey = GetRequestedProductKey(request);

            // Load the whole GemShop node, not just the one product - the client expects the
            // full Products list back so the store view can refresh and drop its spinner.
            Dictionary<string, object> billing = LoadBillingData();
            Dictionary<string, object> gemShopData = Utils.GetDictionary(billing, "GemShop");
            Dictionary<string, object> products = Utils.GetDictionary(gemShopData, "Products");
            Dictionary<string, object> product = string.IsNullOrEmpty(productKey) || products == null
                ? null : Utils.GetDictionary(products, productKey);
            if (product == null)
            {
                Utils.LogWarning("Billing.add_purchased_item: unknown product '" + productKey + "'");
                return;
            }

            // Billing.json "items" is { "<itemId>": <amount> }, where 1 = Gem and 2 = GemAlt.
            // We only model a single gem pool (Player.Gems), so both are summed.
            Dictionary<string, object> items = Utils.GetDictionary(product, "items");
            int total = 0;
            if (items != null)
            {
                foreach (KeyValuePair<string, object> item in items)
                {
                    int itemId;
                    int amount;
                    if (!int.TryParse(item.Key, out itemId) || item.Value == null ||
                        !int.TryParse(item.Value.ToString(), out amount) || amount <= 0)
                    {
                        continue;
                    }
                    switch ((ItemID.Value)itemId)
                    {
                        case ItemID.Value.Gem:
                        case ItemID.Value.GemAlt:
                            total += amount;
                            break;
                        default:
                            // Anything non-gem in a bundle goes through the normal item path.
                            request.Player.Items.Add(itemId);
                            WriteItem(request, itemId);
                            break;
                    }
                }
            }

            if (total > 0)
            {
                request.Player.Gems += total;
                WriteItem(request, (int)ItemID.Value.Gem);
                Utils.LogInfo("Granted " + total + " gems to '" + request.Player.Name + "' (" +
                    Utils.FormatPlayerCode(request.Player.Code) + ") from product " + productKey +
                    ", new total " + request.Player.Gems);
            }

            SavePlayer(request.Player);

            // "buy_count" is the number of purchases already made; the bundle is exhausted once
            // it reaches "limit_count" (-1 on both means unlimited). So this counts up, not down.
            //
            // TODO: the new count is only written into the response, so purchase limits reset
            // when the server restarts. Persisting them needs a per-player counter alongside
            // Player.ShopState rather than mutating the shared Billing.json data.
            object limitCountObj;
            if (product.TryGetValue("limit_count", out limitCountObj) && limitCountObj != null)
            {
                int limitCount;
                int buyCount;
                if (int.TryParse(limitCountObj.ToString(), out limitCount) && limitCount > 0 &&
                    int.TryParse(Utils.GetValue<object>(product, "buy_count", 0).ToString(), out buyCount) &&
                    buyCount < limitCount)
                {
                    product["buy_count"] = buyCount + 1;
                }
            }

            // Mirror the shape of a real purchase response: the client removes these three
            // ClientWork nodes then re-applies them from the payload below. Without Products
            // coming back the store view never finishes refreshing and sits on the spinner.
            Dictionary<string, object> gemShop = request.GetOrCreateDictionary("GemShop");
            gemShop["Products"] = products;
            gemShop["BuyResult"] = new Dictionary<string, object>()
            {
                { "is_send_present", false }
            };
            gemShop["ConfirmReg"] = new List<object>();

            request.Remove("Persistence.System.ticket", "GemShop.Products", "GemShop.BuyResult", "GemShop.ConfirmReg");
        }

        void Act_BillingCancel(GameServerWebRequest request)
        {
            request.Remove("Persistence.System.ticket");
        }
    }
}
