using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Text;
using System.Xml;

namespace QuanLyChiTieu
{
    public class GiaoDich
    {
        public int SoThuTu { get; set; }
        public string MoTa { get; set; }
        public string DanhMuc { get; set; }
        public double SoTien { get; set; }
        public string DonViTienTe { get; set; } 
        public DateTime ThoiGian { get; set; }
    }

    class Program
    {
        private static List<GiaoDich> danhSachGiaoDich = new List<GiaoDich>();
        private static string filePath = "dulieu_giaodich.json"; // Tên file lưu trữ

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Chào mừng đến với chương trình Quản lý Chi tiêu!");
            Console.WriteLine("-------------------------------------------------");

            // Đọc dữ liệu từ file nếu tồn tại
            DocDuLieu();

            bool isRunning = true;

            while (isRunning)
            {
                Console.WriteLine("\n--- MENU ---");
                Console.WriteLine("1. Nhập giao dịch mới");
                Console.WriteLine("2. Sửa giao dịch");
                Console.WriteLine("3. Truy xuất giao dịch");
                Console.WriteLine("4. Gợi ý tối ưu hóa chi tiêu");
                Console.WriteLine("5. Thống kê và báo cáo chi tiêu");
                Console.WriteLine("6. Xuất/Nhập tệp dữ liệu");
                Console.WriteLine("7. Thoát");

                Console.Write("Chọn chức năng: ");
                string luaChon = Console.ReadLine();

                switch (luaChon)
                {
                    case "1":
                        NhapGiaoDichMoi();
                        break;
                    case "2":
                        SuaGiaoDich();
                        break;
                    case "3":
                        TruyXuatGiaoDich();
                        break;
                    case "4":
                        GoiYToiUuHoaChiTieu();
                        break;
                    case "5":
                        ThongKeBaoCao();
                        break;
                    case "6":
                        XuatNhapDuLieu();
                        break;
                    case "7":
                        Console.WriteLine("Đã thoát chương trình. Tạm biệt!");
                        isRunning = false;
                        break;
                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng thử lại.");
                        break;
                }
            }
        }

        // Hàm để đọc dữ liệu trong file
        static void DocDuLieu()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Đọc nội dung file JSON
                    string jsonData = File.ReadAllText(filePath);

                    // Chuyển đổi chuỗi JSON thành danh sách giao dịch
                    danhSachGiaoDich = JsonConvert.DeserializeObject<List<GiaoDich>>(jsonData);

                    Console.WriteLine("Dữ liệu giao dịch đã được tải thành công.");
                }
                else
                {
                    // Tạo file mới nếu không tồn tại
                    Console.WriteLine("Không tìm thấy tệp dữ liệu. Bắt đầu với danh sách rỗng.");
                    // Tạo file mới
                    File.Create(filePath).Close();
                    Console.WriteLine("Tạo tệp mới: " + filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đọc dữ liệu: {ex.Message}");
            }
        }


        // Hàm lưu dữ liệu vào file
        public static void LuuDuLieu()
        {
            try
            {
                // Chuyển đổi danh sách giao dịch thành JSON
                string jsonData = JsonConvert.SerializeObject(danhSachGiaoDich, Newtonsoft.Json.Formatting.Indented);

                // Ghi dữ liệu JSON vào file
                File.WriteAllText(filePath, jsonData);

                Console.WriteLine("Dữ liệu đã được lưu thành công vào file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu dữ liệu: {ex.Message}");
            }
        }

        /// <summary>
        /// Chức năng 1: Thêm giao dịch mới
        /// </summary>
        /// <exception cref="FormatException"></exception>
        static void NhapGiaoDichMoi()
        {
            try
            {
                // Bước 1: Yêu cầu người dùng nhập mô tả và số tiền kèm đơn vị tiền tệ
                Console.WriteLine("\nNhập mô tả giao dịch:");
                string moTa = Console.ReadLine();

                Console.WriteLine("Nhập số tiền (định dạng: 'số tiền đơn vị tiền tệ', ví dụ: 100 USD):");
                string inputTien = Console.ReadLine();
                string[] tienVaDonVi = inputTien.Split(' ');

                if (tienVaDonVi.Length != 2)
                {
                    throw new FormatException("Định dạng không hợp lệ. Vui lòng nhập theo định dạng 'số tiền đơn vị tiền tệ'.");
                }

                double soTien = double.Parse(tienVaDonVi[0]);
                string donViTienTe = tienVaDonVi[1].ToUpper();

                // Gọi API chuyển đổi ngoại tệ nếu đơn vị tiền tệ không phải VND
                if (donViTienTe != "VND")
                {
                    soTien = ChuyenDoiNgoaiTe(soTien, donViTienTe);
                    Console.WriteLine($"Số tiền đã chuyển đổi sang VND: {soTien}");
                    donViTienTe = "VND"; // Đơn vị sau chuyển đổi luôn là VND
                }

                // Gợi ý mô tả giao dịch cùng danh mục và thời gian
                string danhMucGoiY = GoiYDanhMuc(moTa);
                DateTime thoiGianGoiY = DateTime.Now;
                Console.WriteLine($"Hệ thống gợi ý giao dịch như sau:\n - Mô tả: {moTa}\n - Số tiền: {soTien} VND\n - Danh mục: {danhMucGoiY}\n - Thời gian: {thoiGianGoiY}");

                // Hỏi người dùng có đồng ý với gợi ý không
                Console.WriteLine("Bạn có đồng ý với gợi ý này không? (y/n):");
                string dongY = Console.ReadLine().Trim().ToLower();

                string danhMuc;
                DateTime thoiGian;

                if (dongY == "y")
                {
                    danhMuc = danhMucGoiY;
                    thoiGian = thoiGianGoiY;
                }
                else
                {
                    // Yêu cầu người dùng tự nhập danh mục và thời gian nếu không đồng ý
                    Console.WriteLine("Nhập danh mục:");
                    danhMuc = Console.ReadLine();

                    Console.WriteLine("Nhập thời gian giao dịch (yyyy-MM-dd HH:mm:ss):");
                    thoiGian = DateTime.Parse(Console.ReadLine());
                }

                // Gán số thứ tự cho giao dịch
                int soThuTu = danhSachGiaoDich.Count + 1;

                // Thêm giao dịch vào danh sách
                danhSachGiaoDich.Add(new GiaoDich
                {
                    SoThuTu = soThuTu,
                    MoTa = moTa,
                    SoTien = soTien,
                    DonViTienTe = donViTienTe,
                    DanhMuc = danhMuc,
                    ThoiGian = thoiGian
                });

                Console.WriteLine("Giao dịch đã được thêm thành công.");

                // Lưu dữ liệu vào file
                LuuDuLieu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi nhập giao dịch: {ex.Message}");
            }
        }

        static string GoiYDanhMuc(string moTa)
        {
            try
            {
                // Lấy API key từ biến môi trường
                string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                if (string.IsNullOrEmpty(apiKey))
                {
                    Console.WriteLine("API key không được tìm thấy. Vui lòng kiểm tra biến môi trường 'OPENAI_API_KEY'.");
                    return "Không xác định";
                }

                // Đọc dữ liệu mẫu từ file
                if (!File.Exists("du_lieu_mau.txt"))
                {
                    Console.WriteLine("File 'du_lieu_mau.txt' không tồn tại.");
                    return "Không xác định";
                }

                string duLieuMau = File.ReadAllText("du_lieu_mau.txt");

                // Cấu hình prompt cho API
                string prompt = $"Dựa trên dữ liệu mẫu sau đây và mô tả giao dịch, hãy phân loại danh mục phù hợp:\n\nDữ liệu mẫu:\n{duLieuMau}\n\nMô tả giao dịch:\n{moTa}\n\nDanh mục phù hợp:";

                // Gọi API ChatGPT
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var requestBody = new
                    {
                        model = "gpt-3.5-turbo",
                        messages = new[]
                        {
                    new { role = "system", content = "Bạn là một trợ lý phân loại thông tin." },
                    new { role = "user", content = prompt }
                },
                        max_tokens = 50
                    };

                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(requestBody),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = client.PostAsync("https://api.openai.com/v1/chat/completions", content).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Lỗi từ API: {response.StatusCode} - {response.ReasonPhrase}");
                        string errorContent = response.Content.ReadAsStringAsync().Result;
                        Console.WriteLine($"Chi tiết lỗi: {errorContent}");
                        return "Không xác định";
                    }

                    string result = response.Content.ReadAsStringAsync().Result;
                    dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                    return jsonResponse.choices[0].message.content.Trim();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gợi ý danh mục: {ex.Message}");
                return "Không xác định";
            }
        }

        static double ChuyenDoiNgoaiTe(double soTien, string donViTienTe)
        {
            try
            {
                // API Endpoint và API Key 
                string apiKey = "YOUR_API_KEY";
                string url = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/{donViTienTe}";

                // Gọi API ExchangeRate
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(url).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("Không thể lấy tỷ giá từ API.");
                    }

                    string result = response.Content.ReadAsStringAsync().Result;
                    dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(result);

                    // Kiểm tra dữ liệu và lấy tỷ giá sang VND
                    if (jsonResponse.conversion_rates != null && jsonResponse.conversion_rates.VND != null)
                    {
                        double tiGia = (double)jsonResponse.conversion_rates.VND;
                        return soTien * tiGia;
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy tỷ giá cho đơn vị tiền tệ này.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi chuyển đổi ngoại tệ: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Chức năng 2: Sửa giao dịch
        /// </summary>
        static void SuaGiaoDich()
        {
            try
            {
                Console.WriteLine("\nNhập số thứ tự giao dịch cần sửa:");
                int soThuTu = int.Parse(Console.ReadLine());

                var giaoDich = danhSachGiaoDich.FirstOrDefault(g => g.SoThuTu == soThuTu);
                if (giaoDich == null)
                {
                    Console.WriteLine("Không tìm thấy giao dịch.");
                    return;
                }

                Console.WriteLine($"Thông tin giao dịch hiện tại: {JsonConvert.SerializeObject(giaoDich, Newtonsoft.Json.Formatting.Indented)}");

                Console.WriteLine("Nhập mô tả mới (để trống nếu không muốn sửa):");
                string moTaMoi = Console.ReadLine();

                Console.WriteLine("Nhập số tiền mới (để trống nếu không muốn sửa):");
                string soTienMoi = Console.ReadLine();

                Console.WriteLine("Nhập danh mục mới (để trống nếu không muốn sửa):");
                string danhMucMoi = Console.ReadLine();

                if (!string.IsNullOrEmpty(moTaMoi)) giaoDich.MoTa = moTaMoi;
                if (!string.IsNullOrEmpty(soTienMoi)) giaoDich.SoTien = double.Parse(soTienMoi);
                if (!string.IsNullOrEmpty(danhMucMoi)) giaoDich.DanhMuc = danhMucMoi;

                Console.WriteLine("Giao dịch đã được cập nhật.");
                LuuDuLieu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi sửa giao dịch: {ex.Message}");
            }
        }

        /// <summary>
        /// Chức năng 3: Truy xuất giao dịch
        /// </summary>
        static void TruyXuatGiaoDich()
        {
            try
            {
                Console.WriteLine("Chọn cách truy xuất giao dịch:");
                Console.WriteLine("1. Nhập số thứ tự giao dịch.");
                Console.WriteLine("2. Nhập khoảng thời gian.");
                Console.WriteLine("Nhập lựa chọn (1 hoặc 2):");
                string luaChon = Console.ReadLine();

                if (luaChon == "1")
                {
                    Console.WriteLine("Nhập số thứ tự giao dịch:");
                    if (int.TryParse(Console.ReadLine(), out int soThuTu))
                    {
                        var giaoDich = danhSachGiaoDich.FirstOrDefault(g => g.SoThuTu == soThuTu);

                        if (giaoDich != null)
                        {
                            Console.WriteLine("Thông tin giao dịch:");
                            Console.WriteLine(JsonConvert.SerializeObject(giaoDich, Newtonsoft.Json.Formatting.Indented));
                        }
                        else
                        {
                            Console.WriteLine("Không tìm thấy giao dịch với số thứ tự này.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Số thứ tự không hợp lệ.");
                    }
                }
                else if (luaChon == "2")
                {
                    Console.WriteLine("Nhập thời gian bắt đầu (yyyy-MM-dd):");
                    DateTime startTime = DateTime.Parse(Console.ReadLine());

                    Console.WriteLine("Nhập thời gian kết thúc (yyyy-MM-dd):");
                    DateTime endTime = DateTime.Parse(Console.ReadLine());

                    var ketQua = danhSachGiaoDich.Where(g => g.ThoiGian >= startTime && g.ThoiGian <= endTime).ToList();

                    if (ketQua.Any())
                    {
                        Console.WriteLine("Kết quả truy xuất:");
                        foreach (var gd in ketQua)
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(gd, Newtonsoft.Json.Formatting.Indented));
                        }
                    }
                    else
                    {
                        Console.WriteLine("Không tìm thấy giao dịch trong khoảng thời gian này.");
                    }
                }
                else
                {
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi truy xuất giao dịch: {ex.Message}");
            }
        }

        /// <summary>
        /// Chức năng 4: Gợi ý tối ưu hóa chi tiêu
        /// </summary>
        static async void GoiYToiUuHoaChiTieu()
        {
            try
            {
                // Gửi tất cả giao dịch tới API ChatGPT
                string prompt = "Dưới đây là danh sách giao dịch chi tiêu. Hãy đề xuất cách tối ưu hóa chi tiêu cho từng giao dịch, nếu có thể. Nếu có giao dịch không cần thiết hoặc có thể giảm chi phí, hãy chỉ ra.";

                // Tạo một chuỗi JSON để gửi tới API
                var giaoDichJson = JsonConvert.SerializeObject(danhSachGiaoDich);
                prompt += "\n\nDanh sách giao dịch:\n" + giaoDichJson;

                // Gọi API ChatGPT để nhận gợi ý
                string response = await GoiApiChatGPT(prompt);

                // Hiển thị các gợi ý tối ưu hóa chi tiêu
                Console.WriteLine("\nGợi ý tối ưu hóa chi tiêu từ hệ thống:");
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gợi ý tối ưu hóa chi tiêu: {ex.Message}");
            }
        }

        static async Task<string> GoiApiChatGPT(string prompt)
        {
            try
            {
                // Gọi API ChatGPT và nhận phản hồi
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_API_KEY"); // Thay YOUR_API_KEY bằng API key thực của bạn

                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            model = "gpt-3.5-turbo",
                            messages = new[]
                            {
                        new { role = "system", content = "Bạn là một trợ lý tài chính." },
                        new { role = "user", content = prompt }
                            },
                            max_tokens = 500
                        }),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                    string result = await response.Content.ReadAsStringAsync();

                    dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                    return jsonResponse.choices[0].message.content.Trim();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                return "Không thể nhận gợi ý tối ưu hóa từ hệ thống.";
            }
        }

        /// <summary>
        /// Chức năng 5: Thống kê và báo cáo
        /// </summary>
        static void ThongKeBaoCao()
        {
            Console.WriteLine("Chọn phương án thống kê:");
            Console.WriteLine("1. Trong 1 năm");
            Console.WriteLine("2. Trong 6 tháng");
            Console.WriteLine("3. Trong 3 tháng");
            Console.WriteLine("4. Trong 1 tháng");
            Console.WriteLine("5. Trong 1 tuần");
            Console.WriteLine("6. Tùy chỉnh");
            Console.Write("Nhập lựa chọn của bạn (1-6): ");

            int luaChon = int.Parse(Console.ReadLine());
            DateTime startDate = DateTime.Now, endDate = DateTime.Now;

            switch (luaChon)
            {
                case 1:
                    startDate = DateTime.Now.AddYears(-1);
                    break;
                case 2:
                    startDate = DateTime.Now.AddMonths(-6);
                    break;
                case 3:
                    startDate = DateTime.Now.AddMonths(-3);
                    break;
                case 4:
                    startDate = DateTime.Now.AddMonths(-1);
                    break;
                case 5:
                    startDate = DateTime.Now.AddDays(-7);
                    break;
                case 6:
                    Console.Write("Nhập ngày bắt đầu (dd/MM/yyyy): ");
                    startDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Console.Write("Nhập ngày kết thúc (dd/MM/yyyy): ");
                    endDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ!");
                    return;
            }

            if (luaChon != 6) endDate = DateTime.Now;

            // Thực hiện thống kê
            TimeSpan khoangThoiGian = endDate - startDate;
            if (khoangThoiGian.TotalDays > 30)
                ThongKeTheoThang(startDate, endDate);
            else if (khoangThoiGian.TotalDays > 7)
                ThongKeTheoTuan(startDate, endDate);
            else
                ThongKeTheoNgay(startDate, endDate);
        }

        static void ThongKeTheoThang(DateTime startDate, DateTime endDate)
        {
            var giaoDichTrongKhoang = danhSachGiaoDich
                .Where(gd => gd.ThoiGian >= startDate && gd.ThoiGian <= endDate)
                .ToList();

            var chiTieuTheoThang = giaoDichTrongKhoang
                .GroupBy(gd => new { gd.ThoiGian.Year, gd.ThoiGian.Month })
                .Select(group => new
                {
                    Thang = new DateTime(group.Key.Year, group.Key.Month, 1),
                    TongChiTieu = group.Sum(gd => gd.SoTien),
                    TheoDanhMuc = group
                        .GroupBy(gd => gd.DanhMuc)
                        .Select(subGroup => new
                        {
                            DanhMuc = subGroup.Key,
                            TongChiTieu = subGroup.Sum(gd => gd.SoTien)
                        }).ToList()
                }).ToList();

            Console.WriteLine("\nThống kê chi tiêu theo tháng:");
            foreach (var item in chiTieuTheoThang)
            {
                Console.WriteLine($"Tháng {item.Thang:MM/yyyy}: {item.TongChiTieu} VND");
                foreach (var dm in item.TheoDanhMuc)
                {
                    decimal phanTram = ((decimal)dm.TongChiTieu / (decimal)item.TongChiTieu) * 100;
                    Console.WriteLine($"   - {dm.DanhMuc}: {dm.TongChiTieu} VND ({phanTram:F2}%)");
                }
            }
        }

        static void ThongKeTheoTuan(DateTime startDate, DateTime endDate)
        {
            var giaoDichTrongKhoang = danhSachGiaoDich
                .Where(gd => gd.ThoiGian >= startDate && gd.ThoiGian <= endDate)
                .ToList();

            var chiTieuTheoTuan = giaoDichTrongKhoang
                .GroupBy(gd => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(gd.ThoiGian, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                .Select(group => new
                {
                    Tuan = group.Key,
                    TongChiTieu = group.Sum(gd => gd.SoTien),
                    TheoDanhMuc = group
                        .GroupBy(gd => gd.DanhMuc)
                        .Select(subGroup => new
                        {
                            DanhMuc = subGroup.Key,
                            TongChiTieu = subGroup.Sum(gd => gd.SoTien)
                        }).ToList()
                }).ToList();

            Console.WriteLine("\nThống kê chi tiêu theo tuần:");
            foreach (var item in chiTieuTheoTuan)
            {
                Console.WriteLine($"Tuần {item.Tuan}: {item.TongChiTieu} VND");
                foreach (var dm in item.TheoDanhMuc)
                {
                    decimal phanTram = ((decimal)dm.TongChiTieu / (decimal)item.TongChiTieu) * 100;
                    Console.WriteLine($"   - {dm.DanhMuc}: {dm.TongChiTieu} VND ({phanTram:F2}%)");
                }
            }
        }

        static void ThongKeTheoNgay(DateTime startDate, DateTime endDate)
        {
            var giaoDichTrongKhoang = danhSachGiaoDich
                .Where(gd => gd.ThoiGian >= startDate && gd.ThoiGian <= endDate)
                .ToList();

            var chiTieuTheoNgay = giaoDichTrongKhoang
                .GroupBy(gd => gd.ThoiGian.Date)
                .Select(group => new
                {
                    Ngay = group.Key,
                    TongChiTieu = group.Sum(gd => gd.SoTien),
                    TheoDanhMuc = group
                        .GroupBy(gd => gd.DanhMuc)
                        .Select(subGroup => new
                        {
                            DanhMuc = subGroup.Key,
                            TongChiTieu = subGroup.Sum(gd => gd.SoTien)
                        }).ToList()
                }).ToList();

            Console.WriteLine("\nThống kê chi tiêu theo ngày:");
            foreach (var item in chiTieuTheoNgay)
            {
                Console.WriteLine($"Ngày {item.Ngay:dd/MM/yyyy}: {item.TongChiTieu} VND");
                foreach (var dm in item.TheoDanhMuc)
                {
                    decimal phanTram = ((decimal)dm.TongChiTieu / (decimal)item.TongChiTieu) * 100;
                    Console.WriteLine($"   - {dm.DanhMuc}: {dm.TongChiTieu} VND ({phanTram:F2}%)");
                }
            }
        }

        /// <summary>
        /// Chức năng 6: Xuất/Nhập tệp dữ liệu
        /// </summary>
        static void XuatNhapDuLieu()
        {
            Console.WriteLine("\n--- Xuất/Nhập Tệp Dữ Liệu ---");
            Console.WriteLine("1. Nhập tệp dữ liệu");
            Console.WriteLine("2. Xuất tệp dữ liệu");
            Console.Write("Chọn chức năng: ");
            string luaChon = Console.ReadLine();

            if (luaChon == "1")
            {
                NhapTepDuLieu();
            }
            else if (luaChon == "2")
            {
                XuatTepDuLieu();
            }
            else
            {
                Console.WriteLine("Lựa chọn không hợp lệ.");
            }
        }

        // Hàm nhập tệp dữ liệu
        static void NhapTepDuLieu()
        {
            Console.WriteLine("\nChọn cách nhập tệp:");
            Console.WriteLine("1. Nhập tệp mới (Thay thế tệp cũ)");
            Console.WriteLine("2. Nhập tệp mới (Kết hợp với tệp cũ)");
            Console.Write("Chọn chức năng: ");
            string luaChon = Console.ReadLine();

            Console.Write("Nhập đường dẫn tệp cần nhập: ");
            string duongDanTep = Console.ReadLine();

            // Kiểm tra định dạng tệp
            if (!File.Exists(duongDanTep))
            {
                Console.WriteLine("Tệp không tồn tại.");
                return;
            }

            string fileExtension = Path.GetExtension(duongDanTep).ToLower();
            if (fileExtension != ".json" && fileExtension != ".csv" && fileExtension != ".txt")
            {
                Console.WriteLine("Chỉ chấp nhận tệp có định dạng .json, .csv hoặc .txt.");
                return;
            }

            try
            {
                if (luaChon == "1")
                {
                    // Thay thế dữ liệu cũ
                    if (fileExtension == ".json")
                    {
                        string jsonData = File.ReadAllText(duongDanTep);
                        danhSachGiaoDich = JsonConvert.DeserializeObject<List<GiaoDich>>(jsonData);
                    }
                    else if (fileExtension == ".csv" || fileExtension == ".txt")
                    {
                        // Chuyển đổi tệp CSV hoặc TXT thành JSON
                        ChuyenDoiCSV_TXTThanhJSON(duongDanTep);
                    }
                    Console.WriteLine("Tệp đã được nhập và thay thế dữ liệu cũ.");
                }
                else if (luaChon == "2")
                {
                    // Kết hợp với dữ liệu cũ
                    if (fileExtension == ".json")
                    {
                        string jsonData = File.ReadAllText(duongDanTep);
                        var danhSachGiaoDichMoi = JsonConvert.DeserializeObject<List<GiaoDich>>(jsonData);
                        danhSachGiaoDich.AddRange(danhSachGiaoDichMoi);
                    }
                    else if (fileExtension == ".csv" || fileExtension == ".txt")
                    {
                        // Chuyển đổi tệp CSV hoặc TXT thành JSON và kết hợp
                        ChuyenDoiCSV_TXTThanhJSON(duongDanTep);
                    }
                    Console.WriteLine("Tệp đã được nhập và kết hợp dữ liệu.");
                }

                // Lưu lại dữ liệu sau khi nhập
                LuuDuLieu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi nhập tệp dữ liệu: {ex.Message}");
            }
        }

        // Hàm chuyển đổi CSV hoặc TXT thành JSON
        static void ChuyenDoiCSV_TXTThanhJSON(string filePath)
        {
            // Đọc dữ liệu từ tệp CSV/TXT
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var fields = line.Split(',');

                if (fields.Length >= 5)
                {
                    danhSachGiaoDich.Add(new GiaoDich
                    {
                        SoThuTu = int.Parse(fields[0]),
                        MoTa = fields[1],
                        SoTien = double.Parse(fields[2]),
                        DonViTienTe = fields[3],
                        DanhMuc = fields[4],
                        ThoiGian = DateTime.Parse(fields[5])
                    });
                }
            }
            Console.WriteLine("Dữ liệu từ CSV/TXT đã được chuyển đổi thành công.");
        }

        // Hàm xuất tệp dữ liệu
        static void XuatTepDuLieu()
        {
            Console.WriteLine("\nChọn định dạng xuất tệp:");
            Console.WriteLine("1. Xuất dưới định dạng JSON");
            Console.WriteLine("2. Xuất dưới định dạng CSV");
            Console.WriteLine("3. Xuất dưới định dạng TXT");
            Console.Write("Chọn chức năng: ");
            string luaChon = Console.ReadLine();

            string duongDanTepXuat = string.Empty;
            string extension = string.Empty;

            switch (luaChon)
            {
                case "1":
                    duongDanTepXuat = "dulieu_giaodich.json";
                    extension = ".json";
                    break;
                case "2":
                    duongDanTepXuat = "dulieu_giaodich.csv";
                    extension = ".csv";
                    break;
                case "3":
                    duongDanTepXuat = "dulieu_giaodich.txt";
                    extension = ".txt";
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    return;
            }

            try
            {
                // Xuất theo định dạng JSON
                if (extension == ".json")
                {
                    string jsonData = JsonConvert.SerializeObject(danhSachGiaoDich, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(duongDanTepXuat, jsonData);
                }
                // Xuất theo định dạng CSV
                else if (extension == ".csv")
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Số thứ tự, Mô tả, Số tiền, Đơn vị tiền tệ, Danh mục, Thời gian");

                    foreach (var giaoDich in danhSachGiaoDich)
                    {
                        sb.AppendLine($"{giaoDich.SoThuTu},{giaoDich.MoTa},{giaoDich.SoTien},{giaoDich.DonViTienTe},{giaoDich.DanhMuc},{giaoDich.ThoiGian}");
                    }

                    File.WriteAllText(duongDanTepXuat, sb.ToString());
                }
                // Xuất theo định dạng TXT
                else if (extension == ".txt")
                {
                    var sb = new StringBuilder();

                    foreach (var giaoDich in danhSachGiaoDich)
                    {
                        sb.AppendLine($"Số thứ tự: {giaoDich.SoThuTu}");
                        sb.AppendLine($"Mô tả: {giaoDich.MoTa}");
                        sb.AppendLine($"Số tiền: {giaoDich.SoTien}");
                        sb.AppendLine($"Đơn vị tiền tệ: {giaoDich.DonViTienTe}");
                        sb.AppendLine($"Danh mục: {giaoDich.DanhMuc}");
                        sb.AppendLine($"Thời gian: {giaoDich.ThoiGian}\n");
                    }

                    File.WriteAllText(duongDanTepXuat, sb.ToString());
                }

                Console.WriteLine($"Tệp đã được xuất thành công dưới định dạng {extension}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xuất tệp dữ liệu: {ex.Message}");
            }
        }
    }
}
