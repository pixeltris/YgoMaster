using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YgoMaster
{
    class CardFileData
    {
        public int Id;
        public Dictionary<int, CardFileDataCard> CardList;
        public int TotalPage;
        public int TotalCard;
        public int Sort;

        public Dictionary<string, object> RawData;

        public CardFileData(int id, Dictionary<string, object> data)
        {
            Id = id;
            RawData = data;
            CardList = new Dictionary<int, CardFileDataCard>();
            Dictionary<string, object> cardListData = Utils.GetDictionary(data, "card_list");
            foreach (KeyValuePair<string, object> entry in cardListData)
            {
                Dictionary<string, object> cardData = entry.Value as Dictionary<string, object>;
                int cardId = Utils.GetValue<int>(cardData, "card_id");
                CardList[cardId] = new CardFileDataCard()
                {
                    CardId = cardId,
                    Page = Utils.GetValue<int>(cardData, "page"),
                    CardPos = Utils.GetValue<int>(cardData, "card_pos"),
                    AssignPremiumType = Utils.GetValue<int>(cardData, "assign_premium_type"),
                };
            }
            TotalPage = Utils.GetValue<int>(data, "total_page");
            TotalCard = Utils.GetValue<int>(data, "total_card");
            Sort = Utils.GetValue<int>(data, "sort");
        }
    }

    class CardFileDataCard
    {
        public int CardId;
        public int Page;
        public int CardPos;
        public int AssignPremiumType;
    }

    class UserCardFiles
    {
        public Dictionary<int, UserCardFileStatus> Files = new Dictionary<int, UserCardFileStatus>();
        public int ActiveCardFileId;

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["ActiveCardFileId"] = ActiveCardFileId;

            Dictionary<string, object> files = new Dictionary<string, object>();
            foreach (UserCardFileStatus cardFileStatus in Files.Values)
            {
                files[cardFileStatus.Id.ToString()] = cardFileStatus.ToDictionary();
            }
            result["Files"] = files;

            return result;
        }

        public void FromDictionary(Dictionary<string, object> data)
        {
            if (data == null)
            {
                return;
            }
            ActiveCardFileId = Utils.GetValue<int>(data, "ActiveCardFileId");
            Dictionary<string, object> files = Utils.GetDictionary(data, "Files");
            if (files != null)
            {
                foreach (KeyValuePair<string, object> obj in files)
                {
                    Dictionary<string, object> userCardFileData = obj.Value as Dictionary<string, object>;
                    int itemId;
                    if (int.TryParse(obj.Key, out itemId) && userCardFileData != null)
                    {
                        UserCardFileStatus cardFile = new UserCardFileStatus(itemId);
                        cardFile.FromDictionary(userCardFileData);
                        Files[cardFile.Id] = cardFile;
                    }
                }
            }
        }
    }

    class UserCardFileStatus
    {
        public int Id;
        public int SortType;
        public bool HasCollectionBeenExpanded;
        public Dictionary<int, UserCardFileStatusCard> Cards = new Dictionary<int, UserCardFileStatusCard>();

        public UserCardFileStatus(int id)
        {
            Id = id;
        }

        public bool IsComplete()
        {
            return Cards.Values.All(x => x.Have && !x.Dismantled);
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> cards = new Dictionary<string, object>();
            foreach (UserCardFileStatusCard card in Cards.Values)
            {
                cards[card.CardId.ToString()] = card.ToDictionary();
            }

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["SortType"] = SortType;
            result["HasCollectionBeenExpanded"] = HasCollectionBeenExpanded;
            result["Cards"] = cards;
            return result;
        }

        public void FromDictionary(Dictionary<string, object> data)
        {
            Cards.Clear();
            SortType = Utils.GetValue<int>(data, "SortType");
            HasCollectionBeenExpanded = Utils.GetValue<bool>(data, "HasCollectionBeenExpanded");
            Dictionary<string, object> cardsData = Utils.GetDictionary(data, "Cards");
            if (cardsData != null)
            {
                foreach (KeyValuePair<string, object> obj in cardsData)
                {
                    Dictionary<string, object> cardData = obj.Value as Dictionary<string, object>;
                    int cardId;
                    if (int.TryParse(obj.Key, out cardId) && cardData != null)
                    {
                        UserCardFileStatusCard card = new UserCardFileStatusCard(cardId);
                        card.FromDictionary(cardData);
                        Cards[card.CardId] = card;
                    }
                }
            }
        }
    }

    class UserCardFileStatusCard
    {
        public int CardId;
        public CardStyleRarity Style;
        public bool IsNew;
        public bool Have;// Does the collection have this card and has the player seen it added to the collection
        public bool RealHave;// Does the player have this card, regardless of whether the player has seen it added to the collection
        public bool Dismantled;
        public int Page;
        public int Pos;

        public UserCardFileStatusCard(int cardId)
        {
            CardId = cardId;
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["Style"] = (int)Style;
            result["IsNew"] = IsNew;
            result["Have"] = Have;
            result["RealHave"] = RealHave;
            result["Dismantled"] = Dismantled;
            result["Page"] = Page;
            result["Pos"] = Pos;
            return result;
        }

        public void FromDictionary(Dictionary<string, object> data)
        {
            Style = (CardStyleRarity)Utils.GetValue<int>(data, "Style");
            IsNew = Utils.GetValue<bool>(data, "IsNew");
            Have = Utils.GetValue<bool>(data, "Have");
            RealHave = Utils.GetValue<bool>(data, "RealHave");
            Dismantled = Utils.GetValue<bool>(data, "Dismantled");
            Page = Utils.GetValue<int>(data, "Page");
            Pos = Utils.GetValue<int>(data, "Pos");
        }
    }
}
