using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace YgoMaster
{
    /// <summary>
    /// See YgomGame.Card.CardCategoryData
    /// 
    /// This is used for the shop search feature.
    /// Each category is a card archetype with an associated string that the client uses to filter results.
    /// If you want to modify the strings you'll need to modify "External/CardCategory/CardCategory"
    /// (assets/resourcesassetbundle/external/cardcategory/cardcategory.bytes)
    /// </summary>
    class CardCategory
    {
        public int Id;
        public string Name;
        public int Sort;
    }
}
