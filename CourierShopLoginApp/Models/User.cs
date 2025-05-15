using System;

namespace CourierShopLoginApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
        
        // Additional properties from original model
        public string Email { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
