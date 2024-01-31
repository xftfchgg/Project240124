using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models.Dtos;
using WebApplication1.Models.EFModels;

namespace WebApplication1.Controllers
{
    public class ApiController : Controller
    {
        private readonly myDbContext _dbContext;
		private readonly IWebHostEnvironment _host;

		public ApiController(myDbContext context, IWebHostEnvironment host)

        {

            _dbContext = context;
			_host = host;
		}

        public IActionResult Index()
        {
			//return View(); 
			//return Content("<h2>Content,您好</h2>,","text/html",System.Text.Encoding.UTF8);
			System.Threading.Thread.Sleep(3000);
			return Content("<h2>Content,您好</h2>", "text/plain", System.Text.Encoding.UTF8);
        }

        //[HttpPost]
        public IActionResult Cities()
        {
            var cities = _dbContext.Addresses.Select(a => a.City).Distinct();
            return Json(cities);
        }

        public IActionResult Avater(int id=1) {
            Member? member = _dbContext.Members.Find(id);
            if (member != null)
            {
                byte[] img = member.FileData;
                return File(img, "image/jpeg");
            }

            return NotFound();
        }

		public IActionResult Register(UserDto _user)
		{

            if (string.IsNullOrEmpty(_user.Name))
            {
				_user.Name = "訪客";
			}


            string fileName = "empty.jpg";
            if(_user.Avatar != null)
			{
				fileName = _user.Avatar.FileName;
			}
			//取得檔案上傳的實際路徑

			string uploadPath = Path.Combine(_host.WebRootPath, "uploads", fileName);

			//檔案上傳
			using (FileStream fs = new FileStream(uploadPath, FileMode.Create))
			{
				_user.Avatar.CopyTo(fs);
			}
			//return Content($"Hello {_user.Name}, {_user.Age}歲了,電子郵件是{_user.Email}");
			//return Content($"{_user.Avatar?.FileName}-{_user.Avatar?.Length}-{_user.Avatar?.ContentType}");
			//
			return Content($"{_user.Avatar?.FileName}-{_user.Avatar?.Length}-{_user.Avatar?.ContentType}");
		}

		public IActionResult CheckAccount(string name)
		{
            string result = "";

			Member? members = _dbContext.Members.Where(m=>m.Name==name).FirstOrDefault();

			if (members != null) 
            {
				result = "帳號已存在";
			}else
			{
				result = "帳號可使用";
			}
			return Content(result, "text/plain", System.Text.Encoding.UTF8);
			//return Content($"<h2>{result}</h2>", "text/html", System.Text.Encoding.UTF8);
		}

		[HttpPost]
		public IActionResult Spots([FromBody]SearchDto _search) {
			//根據分類編號讀取景點編號
			var spots = _search.categoryId == 0
				? _dbContext.SpotImagesSpots
				: _dbContext.SpotImagesSpots.Where(s => s.CategoryId == _search.categoryId);

			//根據搜尋關鍵字搜尋
			if (!string.IsNullOrEmpty(_search.keyword))
			{
				spots = spots.Where(s => s.SpotTitle.Contains(_search.keyword) || s.SpotDescription.Contains(_search.keyword));
			}

			//排序
			switch (_search.sortBy)
			{
				case "spotTitle":
					spots = _search.sortType == "asc" ? spots.OrderBy(s => s.SpotTitle) : spots.OrderByDescending(s => s.SpotTitle);
					break;
				case "categoryId":
					spots = _search.sortType == "asc" ? spots.OrderBy(s => s.CategoryId) : spots.OrderByDescending(s => s.CategoryId);
					break;
				default:
					spots = _search.sortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending(s => s.SpotId);
					break;
			}

			//分頁
			int TotalCount = spots.Count(); //搜尋出來的資料有幾筆
			int pageSize = _search.pageSize ?? 9;//每頁多少筆
			int TotalPages = (int)Math.Ceiling((decimal)TotalCount / pageSize); //總共幾頁
			int page = _search.page ?? 1 ;//目前第幾頁

			//取得當前頁面的資料
			spots = spots.Skip((page - 1) * pageSize).Take(pageSize);


			//設計要回傳的資料格式
			SpotsPagingDto spotsPaging = new SpotsPagingDto();
			spotsPaging.TotalPages = TotalPages;
			spotsPaging.SpotsResult = spots.ToList();
			return Json(spotsPaging);
		}
	}
}
