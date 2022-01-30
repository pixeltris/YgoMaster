using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    partial class GameServer
    {
        void Act_DeckGetDeckList(GameServerWebRequest request)
        {
            int deckId = GetValue<int>(request.ActParams, "deck_id");
            DeckInfo deck;
            if (request.Player.Decks.TryGetValue(deckId, out deck))
            {
                WriteDeckList(request, deckId);
            }
            request.Remove("DeckList." + deckId);
        }

        void Act_DeckDeleteDeck(GameServerWebRequest request)
        {
            int deckId = GetValue<int>(request.ActParams, "deck_id");
            DeckInfo deck;
            if (request.Player.Decks.TryGetValue(deckId, out deck))
            {
                DeleteDeck(deck);
                request.Player.Decks.Remove(deckId);
                WriteDeckRemoval(request, deckId);
            }
            else
            {
                WriteDeckRemoval(request, deckId, true);
            }
        }

        void Act_DeckUpdate(GameServerWebRequest request)
        {
            DeckInfo deck = new DeckInfo();
            deck.Id = GetValue(request.ActParams, "deck_id", 0);
            deck.Name = GetValue(request.ActParams, "name", "Deck");
            Dictionary<string, object> accessory = GetDictionary(request.ActParams, "accessory");
            Dictionary<string, object> displayCards = GetDictionary(request.ActParams, "pick_cards");
            deck.Accessory.FromDictionary(accessory);
            deck.DisplayCards.FromIndexedDictionary(displayCards);
            Dictionary<string, object> deckListObj = GetDictionary(request.ActParams, "deck_list");
            if (deckListObj != null)
            {
                deck.FromDictionary(deckListObj);
            }
            if (deck.Id == 0)
            {
                deck.Id = request.Player.NextDeckUId++;
                request.Player.Decks[deck.Id] = deck;
                deck.TimeCreated = GetEpochTime();
                deck.GetNewFilePath(decksDirectory);
            }
            else
            {
                DeckInfo existingDeck;
                if (!request.Player.Decks.TryGetValue(deck.Id, out existingDeck))
                {
                    WriteDeckRemoval(request, deck.Id, true);
                    return;
                }
                deck.File = existingDeck.File;
                deck.TimeCreated = existingDeck.TimeCreated;
                request.Player.Decks[deck.Id] = deck;
                if (deck.Name != existingDeck.Name)
                {
                    DeleteDeck(deck);
                    deck.GetNewFilePath(decksDirectory);
                }
                if (accessory == null)
                {
                    deck.Accessory = existingDeck.Accessory;
                }
                if (displayCards == null)
                {
                    deck.DisplayCards.CopyFrom(existingDeck.DisplayCards);
                }
            }
            deck.Accessory.Sanitize(request.Player);
            deck.TimeEdited = GetEpochTime();
            SaveDeck(deck);
            WriteDeck(request, deck.Id);
            WriteDeckList(request, deck.Id);
        }

        void Act_DeckSetDeckAccessory(GameServerWebRequest request)
        {
            int deckId = GetValue<int>(request.ActParams, "deck_id");
            DeckInfo deck;
            if (!request.Player.Decks.TryGetValue(deckId, out deck))
            {
                WriteDeckRemoval(request, deck.Id, true);
                return;
            }
            Dictionary<string, object> args = GetValue(request.ActParams, "param", default(Dictionary<string, object>));
            if (args != null)
            {
                int value;
                if (TryGetValue(args, "deck_case", out value))
                {
                    if (!request.Player.Items.Contains(value) && !UnlockAllItems)
                    {
                        foreach (int id in Enum.GetValues(typeof(ItemID.DECK_CASE)))
                        {
                            if (request.Player.Items.Contains(id))
                            {
                                value = id;
                            }
                        }
                    }
                    deck.Accessory.Box = value;
                }
                if (TryGetValue(args, "protector", out value))
                {
                    deck.Accessory.Sleeve = value;
                }
                if (TryGetValue(args, "field", out value))
                {
                    deck.Accessory.Field = value;
                }
                if (TryGetValue(args, "field_obj", out value))
                {
                    deck.Accessory.FieldObj = value;
                }
                if (TryGetValue(args, "field_avatar_base", out value))
                {
                    deck.Accessory.AvBase = value;
                }
                deck.Accessory.Sanitize(request.Player);
                deck.DisplayCards.FromIndexedDictionary(GetDictionary(args, "pick_cards"));
                SaveDeck(deck);
            }
            WriteDeck(request, deckId);
        }

        void Act_DeckSetFavoriteCards(GameServerWebRequest request)
        {
            request.Player.CardFavorites.FromDictionary(GetDictionary(request.ActParams, "card_list"));
            SavePlayer(request.Player);
            WriteCards_favorite(request);
        }
    }
}
