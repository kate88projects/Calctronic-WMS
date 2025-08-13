﻿using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Item
{
    public class ItemGroupDTO
    {
        public long ItemGroup_Id { get; set; }

        public string ItemGroupCode { get; set; } = "";

        public string Description { get; set; } = "";

        public bool IsActive { get; set; } = true;
    }
}
