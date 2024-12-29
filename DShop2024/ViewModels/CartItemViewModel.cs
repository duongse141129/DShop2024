﻿using DShop2024.Models;

namespace DShop2024.ViewModels
{
	public class CartItemViewModel
	{
		public List<CartItemModel> CartItems { get; set; }
		public decimal GrandTotal { get; set; }
		public decimal ShippingCost { get; set; }
		public string CouponCode { get; set; }
	}
}
