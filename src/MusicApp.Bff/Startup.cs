using System.Web.Http;
using Newtonsoft.Json.Serialization;
using Owin;

namespace MusicApp.Bff
{
    /// <summary>
    /// Lop cau hinh khoi tao duong ong xu ly HTTP cua may chu OWIN (OWIN Pipeline Configuration).
    /// 
    /// Tac dung:
    /// - Dinh tuyen cac yeu cau HTTP den dung Controller tuong ung (Route Configuration).
    /// - Cau hinh bo dinh dang du lieu (Formatters) tra ve dinh dang JSON chuan phong cach camelCase.
    /// - Loai bo dinh dang XML mac dinh de tiet kiem bang thong va tang toc do serialize.
    /// 
    /// Van de giai quyet:
    /// - Thong nhat chuan giao tiep JSON giua BFF va WPF client, tranh cac loi bat dong bo giua cac thuoc tinh PascalCase va camelCase.
    /// - Ho tro ca co che Attribute Routing ([Route(...)]) va Convention Routing truyen thong.
    /// 
    /// Cach thuc van hanh:
    /// - Phuong thuc Configuration duoc goi tu dong boi OWIN host khi WebApp.Start khoi dong.
    /// - Gan HttpConfiguration vao duong ong OWIN thong qua phuong thuc mo rong app.UseWebApi(config).
    /// </summary>
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
