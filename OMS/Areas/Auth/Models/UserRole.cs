namespace OMS.Auth.Models
{
    public class UserRole
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }

        public virtual User User { get; set; }
        public virtual Role Role { get; set; }

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