﻿using Deviax.QueryBuilder;
using System;

namespace NSSLServer
{
    public class ListItem
    {
        public int Id { get; set; }
        public string Gtin { get; set; }
        public string Name { get; set; }
        public int ListId { get; set; }
      //[Column("quantity")]
        public int Amount { get; set; }
        public int BoughtAmount { get; set; }
        public int SortOrder { get; set; }
        public DateTime Changed { get; set; }
        public DateTime Created { get; set; }

        //[ForeignKey(nameof(ListId))]
        //public virtual ShoppingList ShoppingList {get;set;}
        public ListItem() { }
        //public ListItem(int listid, string gtin = null,string name = null,int amount = 1 )
        //{
        //    ListId = listid;
        //    Gtin = gtin;
        //    Name = name;
        //    Amount = amount;
        //}
        public static ListItemTable T = new ListItemTable("listitemtable");
        [PrimaryKey(nameof(Id))]
        public class ListItemTable : Table<ListItemTable>
        {
            public Field Id;
            public Field Gtin;
            public Field Name;
            public Field ListId;
            public Field Amount;
            public Field BoughtAmount;
            public Field SortOrder;
            public Field Changed;
            public Field Created;

            public ListItemTable(string tableAlias = null) : base("public", "list_item", tableAlias)
            {
                Id = F("id");
                Gtin = F("gtin");
                Name = F("name");
                ListId = F("list_id");
                Amount = F("amount");
                BoughtAmount = F("bought_amount");
                SortOrder = F("sort_order");
                Changed = F("changed");
                Created = F("created");
            }
        }

    }
}
