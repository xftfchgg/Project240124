using WebApplication1.Models.EFModels;

namespace WebApplication1.Models.Dtos
{
	public class SpotsPagingDto
	{
		public int TotalPages { get; set; }
		public List<SpotImagesSpot>? SpotsResult { get; set; }
	}
}
