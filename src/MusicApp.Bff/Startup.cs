using System.Web.Http;
using Newtonsoft.Json.Serialization;
using Owin;

namespace MusicApp.Bff
{
    // OWNS: Cấu hình Web API routing và JSON formatters.
    // DOES NOT OWN: Logic request handlers hay vòng đời HTTP listener.
    // CONSTRAINTS: Định dạng response ép buộc dùng camelCase JSON, vô hiệu hoá XML formatter.

    /// <summary>
    /// Lớp khởi tạo cấu hình đường ống xử lý HTTP của OWIN Web API.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Đăng ký HTTP Route (API endpoint) và thay đổi formatter JSON.
    /// 2. Không chịu trách nhiệm: Xử lý hoặc start/stop server thực tế.
    /// 3. Vòng đời trạng thái: Khởi chạy một lần duy nhất lúc khởi động process BFF.
    /// 4. Yêu cầu đặc biệt: Đảm bảo format payload khớp với client WPF (camelCase).
    /// </remarks>
    public class Startup
    {
        /// <summary>
        /// Thiet lap cau hinh he thong cho duong ong xu ly Web API.
        /// </summary>
        /// <param name="app">Doi tuong IAppBuilder dai dien cho chuoi middleware cua OWIN.</param>
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            // Kich hoat Attribute Routing de dinh tuyen linh hoat tren cac Action cua Controller
            config.MapHttpAttributeRoutes();

            // Dinh nghia mau route mac dinh cho cac controller khong su dung attribute
            config.Routes.MapHttpRoute(
                name: "DefaultBffApi",
                routeTemplate: "api/v1/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Loai bo hoan toan XML Formatter, bat buoc he thong chi tra ve du lieu JSON thuan tuy
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Cau hinh JSON Serializer su dung camelCase (tieu de: title, coverImageUrl,...)
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();

            // Tich hop Web API Middleware vao duong ong xu ly cua OWIN
            app.UseWebApi(config);
        }
    }
}
