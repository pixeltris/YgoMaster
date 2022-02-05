using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    partial class GameServer
    {
        void WriteUser(GameServerWebRequest request)
        {
            request.Response["User"] = new Dictionary<string, object>()
            {
                { "profile", new Dictionary<string, object>() {
                    { "name", request.Player.Name },
                    { "rank", request.Player.Rank },
                    { "rate", request.Player.Rate },
                    { "pcode", request.Player.Code },
                    { "level", request.Player.Level },
                    { "exp", request.Player.Exp },
                    { "need_exp", 0 },
                    { "icon_id", request.Player.IconId },
                    { "icon_frame_id", request.Player.IconFrameId },
                    { "follow_num", 0 },
                    { "follower_num", 0 },
                    { "avatar_id", request.Player.AvatarId },
                    { "wallpaper", request.Player.Wallpaper },
                    { "tag", request.Player.TitleTags.ToArray() }
                }}
            };
        }

        void WriteItem(GameServerWebRequest request)
        {
            Dictionary<string, object> have = new Dictionary<string, object>();
            foreach (int item in request.Player.Items)
            {
                have[item.ToString()] = 1;
            }
            foreach (int value in Enum.GetValues(typeof(ItemID.PROFILE_TAG)))
            {
                have[value.ToString()] = 1;
            }
            have[((int)ItemID.CONSUME.ID0001).ToString()] = request.Player.Gems;
            request.Player.CraftPoints.ToDictionary(have);
            request.Player.OrbPoints.ToDictionary(have);
            request.Response["Item"] = new Dictionary<string, object>()
            {
                { "have", have },
            };
        }

        void WriteItem(GameServerWebRequest request, int itemId)
        {
            if (ItemID.GetCategoryFromID(itemId) == ItemID.Category.CARD)
            {
                WriteCards_have(request, itemId);
                return;
            }
            Dictionary<string, object> item = request.GetOrCreateDictionary("Item");
            Dictionary<string, object> have = GetOrCreateDictionary(item, "have");
            switch ((ItemID.CONSUME)itemId)
            {
                case ItemID.CONSUME.ID0001: have[itemId.ToString()] = request.Player.Gems; break;
                case ItemID.CONSUME.ID0003: have[itemId.ToString()] = request.Player.CraftPoints.Get(CardRarity.Normal); break;
                case ItemID.CONSUME.ID0004: have[itemId.ToString()] = request.Player.CraftPoints.Get(CardRarity.Rare); break;
                case ItemID.CONSUME.ID0005: have[itemId.ToString()] = request.Player.CraftPoints.Get(CardRarity.SuperRare); break;
                case ItemID.CONSUME.ID0006: have[itemId.ToString()] = request.Player.CraftPoints.Get(CardRarity.UltraRare); break;
                case ItemID.CONSUME.ID0008: have[itemId.ToString()] = request.Player.OrbPoints.Get(OrbType.Dark); break;
                case ItemID.CONSUME.ID0009: have[itemId.ToString()] = request.Player.OrbPoints.Get(OrbType.Light); break;
                case ItemID.CONSUME.ID0010: have[itemId.ToString()] = request.Player.OrbPoints.Get(OrbType.Earth); break;
                case ItemID.CONSUME.ID0011: have[itemId.ToString()] = request.Player.OrbPoints.Get(OrbType.Water); break;
                case ItemID.CONSUME.ID0012: have[itemId.ToString()] = request.Player.OrbPoints.Get(OrbType.Fire); break;
                case ItemID.CONSUME.ID0013: have[itemId.ToString()] = request.Player.OrbPoints.Get(OrbType.Wind); break;
                default:
                    have[itemId.ToString()] = request.Player.Items.Contains(itemId) ? 1 : 0;
                    break;
            }

        }

        void WriteDeckRemoval(GameServerWebRequest request, int id, bool deckNotFound = false)
        {
            if (deckNotFound)
            {
                LogWarning("Client sent an invalid deck id " + id + ". Removing it.");
            }
            WriteDeck_num_empty(request);
            request.Remove("Deck.list." + id);
            request.Remove("Deck.last_set");
            request.Remove("DeckList." + id);
        }

        void WriteDeck(GameServerWebRequest request)
        {
            Dictionary<string, object> deck = request.GetOrCreateDictionary("Deck");
            Dictionary<string, object> list = new Dictionary<string, object>();
            foreach (DeckInfo deckInfo in request.Player.Decks.Values)
            {
                WriteDeck_list_item(request, list, deckInfo);
            }
            deck["list"] = list;
            deck["deckMax"] = NumDeckSlots;// Probably a max slot limit when they introduce buying more slots? (deckMax:50 deckLimit:20)
            deck["deckLimit"] = NumDeckSlots;// The deck limit displayed on the top right of the deck selection screen
            WriteDeck_num_empty(request);
        }

        void WriteDeck(GameServerWebRequest request, int id)
        {
            DeckInfo deckInfo;
            if (!request.Player.Decks.TryGetValue(id, out deckInfo))
            {
                return;
            }
            Dictionary<string, object> deck = request.GetOrCreateDictionary("Deck");
            Dictionary<string, object> list = new Dictionary<string, object>();
            WriteDeck_list_item(request, list, deckInfo);
            deck["list"] = list;
            WriteDeck_num_empty(request);
            WriteDeck_last_set(request, id);
        }

        void WriteDeck_list_item(GameServerWebRequest request, Dictionary<string, object> list, DeckInfo deckInfo)
        {
            list[deckInfo.Id.ToString()] = new Dictionary<string, object>()
            {
                { "deck_id", deckInfo.Id },
                { "name", deckInfo.Name },
                { "status", 0 },
                { "et", deckInfo.TimeEdited },
                { "ct", deckInfo.TimeCreated },
                { "accessory", deckInfo.Accessory.ToDictionary() },
                { "pick_cards", deckInfo.DisplayCards.ToIndexDictionary() }
            };
        }

        void WriteDeck_num_empty(GameServerWebRequest request)
        {
            Dictionary<string, object> deck = request.GetOrCreateDictionary("Deck");
            deck["num"] = request.Player.Decks.Count;
            deck["empty"] = Math.Max(0, NumDeckSlots - request.Player.Decks.Count);
        }

        void WriteDeck_last_set(GameServerWebRequest request, int id)
        {
            Dictionary<string, object> deck = request.GetOrCreateDictionary("Deck");
            deck["last_set"] = id;
        }

        void WriteDeckList(GameServerWebRequest request)
        {
            foreach (DeckInfo deck in request.Player.Decks.Values)
            {
                WriteDeckList(request, deck.Id);
            }
        }

        void WriteDeckList(GameServerWebRequest request, int deckId)
        {
            Dictionary<string, object> deckList = request.GetOrCreateDictionary("DeckList");
            DeckInfo deckInfo;
            if (request.Player.Decks.TryGetValue(deckId, out deckInfo))
            {
                deckList[deckId.ToString()] = deckInfo.ToDictionary();
            }
        }

        void WriteCards(GameServerWebRequest request)
        {
            WriteCards_have(request);
            WriteCards_favorite(request);
        }

        void WriteCards_have(GameServerWebRequest request)
        {
            WriteCards_have(request, new HashSet<int>(request.Player.Cards.GetIDs()));
        }

        void WriteCards_have(GameServerWebRequest request, int cardId)
        {
            WriteCards_have(request, new HashSet<int>() { cardId });
        }

        void WriteCards_have(GameServerWebRequest request, HashSet<int> cardIds)
        {
            Dictionary<string, object> cards = request.GetOrCreateDictionary("Cards");
            Dictionary<string, object> ownedCards = GetOrCreateDictionary(cards, "have");
            foreach (int cardId in cardIds)
            {
                Dictionary<string, object> cardData = request.Player.Cards.CardToDictionary(cardId, CardRare);
                if (cardData != null)
                {
                    ownedCards[cardId.ToString()] = cardData;
                }
            }
        }

        void WriteCards_favorite(GameServerWebRequest request)
        {
            Dictionary<string, object> cards = request.GetOrCreateDictionary("Cards");
            cards["favorite"] = request.Player.CardFavorites.ToDictionary();
        }

        void WriteSolo(GameServerWebRequest request)
        {
            WriteSolo_deck_info(request);
            WriteSolo_cleared(request);
        }

        void WriteSolo_cleared(GameServerWebRequest request)
        {
            Dictionary<string, object> solo = request.GetOrCreateDictionary("Solo");
            solo["cleared"] = request.Player.SoloChaptersToDictionary();
        }

        void WriteSolo_deck_info(GameServerWebRequest request)
        {
            DeckInfo deck = request.Player.Duel.GetDeck(GameMode.SoloSingle);
            Dictionary<string, object> solo = request.GetOrCreateDictionary("Solo");
            solo["deck_info"] = new Dictionary<string, object>()
            {
                { "deck_id", deck != null ? deck.Id : 0 },
                { "valid", deck != null ? deck.IsValid(request.Player) : false },
                { "possession", true }//request.Player.Duel.IsMyDeck }
            };
        }
    }
}
