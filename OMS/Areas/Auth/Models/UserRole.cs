namespace OMS.Auth.Models
{
    public class UserRole
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }

        public User User { get; set; }
        public Role Role { get; set; }

        public UserRole()
        {

        }

        public UserRole(User user, Role role)
        {
            User = user;
            Role = role;
            UserId = user.Id;
            RoleId = role.Id;
        }
    }
}