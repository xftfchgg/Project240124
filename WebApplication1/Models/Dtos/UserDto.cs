namespace WebApplication1.Models.Dtos
{
	public class UserDto
	{
		public string? Name { get; set; }
		public int? Age { get; set; }
		public string? Email { get; set; }
		public IFormFile? Avatar { get; set; }
	}
}
