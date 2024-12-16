using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace QuanLyChiTieu
{
    public class TaiChinh
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

        public class QuanLyNganSach
        {
            public string ThangNam { get; set; }
            public double HanMuc { get; set; }
        }
    }

    class Program
    {
        private static List<TaiChinh.GiaoDich> danhSachGiaoDich = new List<TaiChinh.GiaoDich>();
        private static string filePath = "dulieu_giaodich.json";

        private static List<TaiChinh.QuanLyNganSach> danhSachHanMuc = new List<TaiChinh.QuanLyNganSach>();
        private static string hanMucFilePath = "dulieu_hanmuc.json";


        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Expense Management Program!");
            Console.WriteLine("-------------------------------------------------");

            // Đọc dữ liệu từ file nếu tồn tại
            DocDuLieu();
            DocDuLieuHanMuc();

            bool isRunning = true;

            while (isRunning)
            {
                Console.WriteLine("\n--- MENU ---");
                Console.WriteLine("1. Enter a new transaction");
                Console.WriteLine("2. Edit/Delete a transaction");
                Console.WriteLine("3. Retrieve transaction");
                Console.WriteLine("4. Manage and check budget");
                Console.WriteLine("5. Generate expense reports");
                Console.WriteLine("6. Export/Import data file");
                Console.WriteLine("7. Exit");

                Console.Write("Select a function: ");
                string luaChon = Console.ReadLine();

                switch (luaChon)
                {
                    case "1":
                        NhapGiaoDichMoi().Wait(); // Gọi hàm xử lý giao dịch
                        Console.WriteLine("\nPress any key to return to the menu....");
                        Console.ReadKey(); // Chờ người dùng nhập phím trước khi quay lại menu
                        break;
                    case "2":
                        SuaGiaoDich();
                        break;
                    case "3":
                        TruyXuatGiaoDich();
                        break;
                    case "4":
                        QuanLyVaKiemTraHanMuc();
                        break;
                    case "5":
                        ThongKeBaoCao();
                        break;
                    case "6":
                        XuatNhapDuLieu();
                        break;
                    case "7":
                        Console.WriteLine("Program has exited. Goodbye!");
                        isRunning = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
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
                    danhSachGiaoDich = JsonConvert.DeserializeObject<List<TaiChinh.GiaoDich>>(jsonData);

                    Console.WriteLine("Transaction data has been successfully loaded");
                }
                else
                {
                    // Tạo file mới nếu không tồn tại
                    Console.WriteLine("Data file not found. Starting with an empty list.");
                    // Tạo file mới
                    File.Create(filePath).Close();
                    Console.WriteLine("Create a new file: " + filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading data: {ex.Message}");
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

                Console.WriteLine("Data has been successfully saved to the file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        /// <summary>
        /// Chức năng 1: Thêm giao dịch mới
        /// </summary>
        /// <exception cref="FormatException"></exception>
        static async Task NhapGiaoDichMoi()
        {
            if (danhSachGiaoDich == null)
            {
                danhSachGiaoDich = new List<TaiChinh.GiaoDich>();
            }
            try
            {
                // Bước 1: Nhập mô tả và số tiền
                Console.WriteLine("\nEnter transaction description (at least 5 words):");
                string moTa = Console.ReadLine();

                // Tách chuỗi thành mảng các từ
                string[] words = moTa.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Kiểm tra số lượng từ
                while (words.Length < 5)
                {
                    Console.WriteLine("Re-enter transaction description (at least 5 words):");
                    moTa = Console.ReadLine();

                    // Cập nhật lại mảng words với mô tả mới
                    words = moTa.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }

                Console.WriteLine("Enter amount (format: 'amount currency unit', e.g., 1000 VND):");
                string inputTien = Console.ReadLine();
                string[] tienVaDonVi = inputTien.Split(' ');

                if (tienVaDonVi.Length != 2)
                {
                    throw new FormatException("Invalid format. Please enter in the format 'amount currency unit'.");
                }

                double soTien = double.Parse(tienVaDonVi[0]);
                string donViTienTe = tienVaDonVi[1].ToUpper();

                // Gọi API chuyển đổi ngoại tệ nếu cần
                if (donViTienTe != "VND")
                {
                    soTien = ChuyenDoiNgoaiTe(soTien, donViTienTe);
                    Console.WriteLine($"Amount converted to VND: {soTien}");
                    donViTienTe = "VND";
                }

                // Bước 2: Gợi ý danh mục và thời gian
                string danhMucGoiY = await GoiYDanhMuc(moTa);
                DateTime thoiGianGoiY = DateTime.Now;
                string thoiGianChuoi = thoiGianGoiY.ToString("yyyy-MM-dd HH:mm:ss");
                Console.WriteLine($"Transaction suggestion system:\n - Description: {moTa}\n - Amount: {soTien} VND\n - Category: {danhMucGoiY}\n - Time: {thoiGianChuoi}");

                // Bước 3: Nhập ý kiến người dùng
                string danhMuc;
                DateTime thoiGian;

                while (true)
                {
                    Console.WriteLine("Do you agree with this suggestion? (y/n):");
                    string dongY = Console.ReadLine().Trim().ToLower();

                    if (dongY == "y")
                    {
                        danhMuc = danhMucGoiY;
                        thoiGian = thoiGianGoiY;
                        break;
                    }
                    else if (dongY == "n")
                    {
                        // Bắt buộc nhập danh mục không được trống
                        while (true)
                        {
                            Console.WriteLine("Enter category:");
                            danhMuc = Console.ReadLine().Trim();
                            if (!string.IsNullOrEmpty(danhMuc))
                            {
                                break; // Dữ liệu hợp lệ, thoát vòng lặp
                            }
                            else
                            {
                                Console.WriteLine("Category cannot be empty. Please try again.");
                            }
                        }

                        // Bắt buộc nhập thời gian giao dịch hợp lệ
                        while (true)
                        {
                            Console.WriteLine("Enter transaction time (yyyy-MM-dd HH:mm:ss):");
                            string inputThoiGian = Console.ReadLine().Trim();

                            if (DateTime.TryParseExact(inputThoiGian, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out thoiGian))
                            {
                                break; // Dữ liệu hợp lệ, thoát vòng lặp
                            }
                            else
                            {
                                Console.WriteLine("Invalid datetime format. Please try again.");
                            }
                        }
                        break; // Thoát vòng lặp lớn khi hoàn tất
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Please enter 'y' or 'n'.");
                    }
                }
                string thoiGianThuc = thoiGian.ToString("yyyy-MM-dd HH:mm:ss");

                // Bước 4: Thêm giao dịch vào danh sách
                int soThuTu = danhSachGiaoDich.Count + 1;
                danhSachGiaoDich.Add(new TaiChinh.GiaoDich
                {
                    SoThuTu = soThuTu,
                    MoTa = moTa,
                    SoTien = soTien,
                    DonViTienTe = donViTienTe,
                    DanhMuc = danhMuc,
                    ThoiGian = DateTime.Parse(thoiGianThuc)
                });

                Console.WriteLine("Transaction has been successfully added");
                LuuDuLieu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error entering transaction: {ex.Message}");
            }
        }


        public static async Task<string> GoiYDanhMuc(string moTa)
        {
            // API key và URL của Google Cloud Natural Language API
            string apiKey = "...";
            string url = "https://language.googleapis.com/v1/documents:classifyText?key=" + apiKey;

            // Thêm text để đảm bảo số lượng token
            string text = $"{moTa} with my family, including my dad, my mom, my son, my sister and my brother";

            // Tạo đối tượng JSON yêu cầu
            var requestData = new
            {
                document = new
                {
                    type = "PLAIN_TEXT",
                    content = text
                }
            };

            // Gọi API và nhận phản hồi
            var response = await CallGoogleCloudAPI(url, requestData);

            // Nếu phản hồi không rỗng, phân tích kết quả
            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    var categoryKeywords = new Dictionary<string, List<string>>
        {
            { "Food", new List<string> { "food", "drink", "beverage", "cooking", "recipes", "grocery","restaurants" } },
            { "Housing & Lifestyle", new List<string> {"shopping","computers", "electronics","finance","home","garden","internet","telecom","jobs","communities","news","law","society","communites" } },
            { "Transportation", new List<string> { "autos","motor","vehicles","gas","fueling","retailers","dealers","travel","transportation" } },
            { "Education", new List<string> { "books","literature","education" } },
            { "Healthcare & Beauty", new List<string> { "fitness","health","medical","beauty","body","hair","weight","fashion","insurance" } },
            { "Entertainment & Hobbies", new List<string> {"sports","books","hobbies","leisure","games","pets","event","celebrities","humor","entertainment","movies","music","audio","art","design" } },
            { "Work & Career", new List<string> {"business", "industrial", "trade","investing","jobs" } }
        };

                    // Phân tích phản hồi JSON
                    var jsonResponse = JObject.Parse(response);
                    var categories = jsonResponse["categories"];

                    if (categories != null && categories.Any())
                    {
                        // Lấy category có confidence cao nhất
                        var highestConfidenceCategory = categories
                            .OrderByDescending(category => (float)category["confidence"])
                            .FirstOrDefault();

                        if (highestConfidenceCategory != null)
                        {
                            string categoryName = highestConfidenceCategory["name"].ToString();

                            // Tìm danh mục tương đồng nhất dựa trên từ khóa
                            return FindBestMatch(categoryName, categoryKeywords);
                        }
                    }
                    return "Unknown"; // Nếu không tìm thấy danh mục phù hợp
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing response: " + ex.Message);
                    return "Unknown";
                }
            }

            return "No response from API";
        }

        // Hàm tìm danh mục tương đồng nhất dựa trên từ khóa
        public static string FindBestMatch(string inputCategory, Dictionary<string, List<string>> categoryKeywords)
        {
            var tokens = inputCategory.ToLower().Split(new[] { ' ', '/', '&', '-', '_', ',' }, StringSplitOptions.RemoveEmptyEntries);

            string bestMatch = null;
            int maxMatches = 0;

            foreach (var entry in categoryKeywords)
            {
                int matchCount = entry.Value.Intersect(tokens).Count(); // Đếm từ khóa khớp
                if (matchCount > maxMatches)
                {
                    maxMatches = matchCount;
                    bestMatch = entry.Key;
                }
            }

            return bestMatch ?? "Unknown"; // Nếu không tìm thấy danh mục phù hợp
        }

        // Hàm gọi Google Cloud Natural Language API
        public static async Task<string> CallGoogleCloudAPI(string url, object requestData)
        {
            using (var client = new HttpClient())
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return $"Error: {response.StatusCode}. {await response.Content.ReadAsStringAsync()}";
                }
            }
        }
        static double ChuyenDoiNgoaiTe(double soTien, string donViTienTe)
        {
            try
            {
                // API Endpoint và API Key 
                string apiKey = "...";
                string url = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/{donViTienTe}";

                // Gọi API ExchangeRate
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(url).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("Unable to retrieve exchange rate from the API.");
                    }

                    string result = response.Content.ReadAsStringAsync().Result;
                    dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(result);

                    // Kiểm tra dữ liệu và lấy tỷ giá sang VND
                    if (jsonResponse.conversion_rates != null && jsonResponse.conversion_rates.VND != null)
                    {
                        double tiGia = (double)jsonResponse.conversion_rates.VND;
                        return Math.Round(soTien * tiGia, 0);
                    }
                    else
                    {
                        throw new Exception("Exchange rate for this currency not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting foreign currency: {ex.Message}");
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
                Console.WriteLine("\n--- Edit/Delete Transaction---");
                if (danhSachGiaoDich == null || danhSachGiaoDich.Count == 0)
                {
                    Console.WriteLine("Transaction list is empty.");
                    return;
                }

                // Yêu cầu người dùng chọn giao dịch cần sửa hoặc xóa
                Console.Write("Enter the transaction serial number to edit or delete: ");
                if (!int.TryParse(Console.ReadLine(), out int soThuTu) || soThuTu <= 0 || soThuTu > danhSachGiaoDich.Count)
                {
                    Console.WriteLine("Invalid number!");
                    return;
                }

                var giaoDich = danhSachGiaoDich.FirstOrDefault(gd => gd.SoThuTu == soThuTu);
                if (giaoDich == null)
                {
                    Console.WriteLine("Transaction with the entered serial number not found.");
                    return;
                }

                Console.WriteLine("\nWhat action would you like to perform?");
                Console.WriteLine("1. Edit transaction");
                Console.WriteLine("2. Delete transaction");
                Console.Write("Select: ");
                string luaChon = Console.ReadLine();

                if (luaChon == "1") // Sửa giao dịch
                {
                    Console.WriteLine("Enter new description (or press Enter to keep the same):");
                    string moTaMoi = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(moTaMoi))
                    {
                        giaoDich.MoTa = moTaMoi;
                    }

                    Console.WriteLine("Enter the new amount (or press Enter to keep the same):");
                    string soTienMoiStr = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(soTienMoiStr) && double.TryParse(soTienMoiStr, out double soTienMoi))
                    {
                        giaoDich.SoTien = soTienMoi;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input! or null. Your amount will be kept the same");
                    }

                    Console.WriteLine("Enter the new category (or press Enter to keep the same):");
                    string danhMucMoi = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(danhMucMoi))
                    {
                        giaoDich.DanhMuc = danhMucMoi;
                    }

                    Console.WriteLine("Enter the new time (yyyy-MM-dd HH:mm:ss, or press Enter to keep the same):");
                    string thoiGianMoiStr = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(thoiGianMoiStr) && DateTime.TryParse(thoiGianMoiStr, out DateTime thoiGianMoi))
                    {
                        giaoDich.ThoiGian = thoiGianMoi;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input! or null. Your time will be kept the same");
                    }

                    Console.WriteLine("Transaction has been successfully updated.");
                }
                else if (luaChon == "2") // Xóa giao dịch
                {
                    danhSachGiaoDich.Remove(giaoDich);
                    Console.WriteLine("Transaction has been successfully deleted.");

                    // Cập nhật lại số thứ tự
                    for (int i = 0; i < danhSachGiaoDich.Count; i++)
                    {
                        danhSachGiaoDich[i].SoThuTu = i + 1;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid selection!");
                }

                // Lưu dữ liệu vào file sau khi sửa hoặc xóa
                LuuDuLieu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error editing transaction: {ex.Message}");
            }
        }
        /// <summary>
        /// Chức năng 3: Truy xuất giao dịch
        /// </summary>
        static void TruyXuatGiaoDich()
        {
            if (danhSachGiaoDich == null || danhSachGiaoDich.Count == 0)
            {
                Console.WriteLine("Transaction list is empty.");
                return;
            }
            // Bắt đầu khối lệnh có thể xảy ra lỗi
            try
            {
                // Hiển thị tùy chọn truy xuất giao dịch
                Console.WriteLine("Choose a method to retrieve transactions:");

                // Hiển thị: Chọn cách truy xuất giao dịch bằng số thứ tự
                Console.WriteLine("1. Enter the transaction number.");

                // Hiển thị: Chọn cách truy xuất giao dịch bằng khoảng thời gian
                Console.WriteLine("2. Enter a time range.");

                // Hiển thị: Yêu cầu nhập lựa chọn
                Console.WriteLine("Enter your choice (1 or 2):");

                // Lấy lựa chọn từ người dùng
                string luaChon = Console.ReadLine();

                // Kiểm tra nếu người dùng chọn phương án 1
                if (luaChon == "1")
                {
                    // Hiển thị: Yêu cầu nhập số thứ tự giao dịch
                    Console.WriteLine("Enter the transaction number:");

                    // Kiểm tra và chuyển đổi đầu vào thành số nguyên
                    if (int.TryParse(Console.ReadLine(), out int soThuTu))
                    {
                        // Tìm giao dịch theo số thứ tự
                        var giaoDich = danhSachGiaoDich.FirstOrDefault(g => g.SoThuTu == soThuTu);

                        // Kiểm tra nếu tìm thấy giao dịch
                        if (giaoDich != null)
                        {
                            // Hiển thị: Thông tin giao dịch tìm thấy
                            Console.WriteLine("Transaction details:");

                            // Hiển thị giao dịch dưới dạng JSON
                            Console.WriteLine(JsonConvert.SerializeObject(giaoDich, Newtonsoft.Json.Formatting.Indented));
                        }
                        else
                        {
                            // Hiển thị: Không tìm thấy giao dịch
                            Console.WriteLine("No transaction found with this number.");
                        }
                    }
                    else
                    {
                        // Hiển thị: Số thứ tự không hợp lệ
                        Console.WriteLine("Invalid transaction number.");
                    }
                }
                // Kiểm tra nếu người dùng chọn phương án 2
                else if (luaChon == "2")
                {
                    // Hiển thị: Yêu cầu nhập thời gian bắt đầu
                    Console.WriteLine("Enter the start date (yyyy-MM-dd):");

                    // Lấy và chuyển đổi đầu vào thành kiểu ngày tháng
                    DateTime startTime = DateTime.Parse(Console.ReadLine());

                    // Hiển thị: Yêu cầu nhập thời gian kết thúc
                    Console.WriteLine("Enter the end date (yyyy-MM-dd):");

                    // Lấy và chuyển đổi đầu vào thành kiểu ngày tháng
                    DateTime endTime = DateTime.Parse(Console.ReadLine());

                    // Lọc danh sách giao dịch theo khoảng thời gian
                    var ketQua = danhSachGiaoDich.Where(g => g.ThoiGian >= startTime && g.ThoiGian <= endTime).ToList();

                    // Kiểm tra nếu có giao dịch trong khoảng thời gian
                    if (ketQua.Any())
                    {
                        // Hiển thị: Danh sách giao dịch trong khoảng thời gian
                        Console.WriteLine("Retrieved transactions:");
                        foreach (var gd in ketQua)
                        {
                            // Hiển thị từng giao dịch
                            Console.WriteLine(JsonConvert.SerializeObject(gd, Newtonsoft.Json.Formatting.Indented));
                        }
                    }
                    else
                    {
                        // Hiển thị: Không tìm thấy giao dịch trong khoảng thời gian
                        Console.WriteLine("No transactions found in this time range.");
                    }
                }
                else
                {
                    // Hiển thị: Lựa chọn không hợp lệ
                    Console.WriteLine("Invalid choice.");
                }
            }
            // Bắt lỗi nếu xảy ra ngoại lệ trong quá trình thực hiện
            catch (Exception ex)
            {
                // Hiển thị: Lỗi khi truy xuất giao dịch
                Console.WriteLine($"Error retrieving transactions: {ex.Message}");
            }
        }


        /// <summary>
        /// Chức năng 4: Quản lý và kiểm tra hạn mức
        /// </summary>
        static void QuanLyVaKiemTraHanMuc()
        {

            if (danhSachGiaoDich == null || danhSachGiaoDich.Count == 0)
            {
                Console.WriteLine("Transaction list is empty.");
                return;
            }
            // Hiển thị tiêu đề chức năng quản lý và kiểm tra hạn mức
            Console.WriteLine("\n--- Manage and Check Budget Limits ---");

            // Hiển thị tùy chọn nhập hoặc chỉnh sửa hạn mức
            Console.WriteLine("1. Enter or edit budget limit");

            // Hiển thị tùy chọn kiểm tra hạn mức theo tháng
            Console.WriteLine("2. Check budget limit by month");

            // Yêu cầu người dùng chọn chức năng
            Console.Write("Choose an option: ");
            string luaChon = Console.ReadLine();

            // Xử lý lựa chọn của người dùng
            switch (luaChon)
            {
                case "1":
                    // Chọn chức năng quản lý hạn mức
                    QuanLyHanmuc();
                    break;
                case "2":
                    // Chọn chức năng kiểm tra hạn mức
                    KiemTraHanmuc();
                    break;
                default:
                    // Thông báo lựa chọn không hợp lệ
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
        // Hàm đọc dữ liệu hạn mức
        static void DocDuLieuHanMuc()
        {
            try
            {
                // Kiểm tra xem tệp dữ liệu hạn mức có tồn tại không
                if (File.Exists(hanMucFilePath))
                {
                    // Đọc dữ liệu từ tệp JSON
                    string jsonData = File.ReadAllText(hanMucFilePath);
                    // Giải mã dữ liệu JSON thành danh sách hạn mức
                    danhSachHanMuc = JsonConvert.DeserializeObject<List<TaiChinh.QuanLyNganSach>>(jsonData);
                    // Thông báo thành công khi tải dữ liệu
                    Console.WriteLine("The budget limit data has been successfully loaded.");
                }
                else
                {
                    // Thông báo nếu không tìm thấy tệp dữ liệu và tạo tệp mới
                    Console.WriteLine("No budget limit data file found. Starting with an empty list.");
                    File.Create(hanMucFilePath).Close();
                    Console.WriteLine("Created a new budget limit file: " + hanMucFilePath);
                }
            }
            catch (Exception ex)
            {
                // Thông báo lỗi khi đọc dữ liệu
                Console.WriteLine($"Error reading data: {ex.Message}");
            }
        }
        // Hàm lưu dữ liệu hạn mức
        public static void LuuDuLieuHanMuc()
        {
            try
            {
                // Chuyển đổi danh sách hạn mức thành JSON
                string jsonData = JsonConvert.SerializeObject(danhSachHanMuc, Newtonsoft.Json.Formatting.Indented);

                // Ghi dữ liệu JSON vào tệp
                File.WriteAllText(hanMucFilePath, jsonData);

                // Thông báo khi lưu dữ liệu thành công
                Console.WriteLine("The data has been successfully saved to the file.");
            }
            catch (Exception ex)
            {
                // Thông báo lỗi khi lưu dữ liệu
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }
        // Hàm quản lý hạn mức
        static void QuanLyHanmuc()
        {
            // Đảm bảo rằng danhSachHanMuc không phải là null
            if (danhSachHanMuc == null)
            {
                danhSachHanMuc = new List<TaiChinh.QuanLyNganSach>();
            }

            // Yêu cầu người dùng nhập tháng muốn quản lý hạn mức
            Console.WriteLine("\nEnter the month you want to manage the budget limit (format: yyyy-MM):");
            string thangNam = Console.ReadLine();

            // Kiểm tra định dạng tháng nhập vào
            if (!DateTime.TryParseExact(thangNam, "yyyy-MM", null, DateTimeStyles.None, out _))
            {
                // Thông báo nếu định dạng tháng không hợp lệ
                Console.WriteLine("Invalid month format. Please enter in yyyy-MM format.");
                return;
            }

            // Yêu cầu người dùng nhập hạn mức chi tiêu cho tháng
            Console.WriteLine($"Enter the spending limit for {thangNam} (VND):");
            if (!double.TryParse(Console.ReadLine(), out double hanMuc) || hanMuc < 0)
            {
                // Thông báo nếu hạn mức không hợp lệ
                Console.WriteLine("Invalid budget limit. Please enter a positive number.");
                return;
            }

            // Kiểm tra xem hạn mức đã tồn tại cho tháng này chưa
            var hanMucTonTai = danhSachHanMuc.FirstOrDefault(h => h.ThangNam == thangNam);
            if (hanMucTonTai != null)
            {
                // Cập nhật hạn mức nếu đã tồn tại
                hanMucTonTai.HanMuc = hanMuc;
            }
            else
            {
                // Thêm hạn mức mới nếu chưa có
                danhSachHanMuc.Add(new TaiChinh.QuanLyNganSach { ThangNam = thangNam, HanMuc = hanMuc });
            }

            // Lưu dữ liệu hạn mức
            LuuDuLieuHanMuc();
            // Thông báo đã cập nhật hạn mức
            Console.WriteLine($"The budget limit for {thangNam} has been updated to {hanMuc} VND.");
        }


        // Hàm kiểm tra hạn mức       
        static void KiemTraHanmuc()
        {
            if (danhSachHanMuc == null)
            {
                danhSachHanMuc = new List<TaiChinh.QuanLyNganSach>();
            }

            // Yêu cầu người dùng nhập tháng muốn kiểm tra hạn mức
            Console.WriteLine("\nEnter the month you want to check the budget limit for (format: yyyy-MM):");
            string thangNam = Console.ReadLine();

            // Kiểm tra định dạng tháng nhập vào
            if (!DateTime.TryParseExact(thangNam, "yyyy-MM", null, DateTimeStyles.None, out _))
            {
                // Thông báo nếu định dạng tháng không hợp lệ
                Console.WriteLine("Invalid month format. Please enter in yyyy-MM format.");
                return;
            }

            // Tính tổng chi tiêu trong tháng
            var tongChiTieu = danhSachGiaoDich
                .Where(gd => gd.ThoiGian.ToString("yyyy-MM") == thangNam)
                .Sum(gd => gd.SoTien);

            // Kiểm tra xem có hạn mức cho tháng này không
            var hanMuc = danhSachHanMuc.FirstOrDefault(h => h.ThangNam == thangNam);

            if (hanMuc != null)
            {
                // Tính tỷ lệ chi tiêu so với hạn mức
                double tiLe = (tongChiTieu / hanMuc.HanMuc) * 100;

                // Thông báo nếu vượt quá 100% hạn mức
                if (tiLe >= 100)
                {
                    Console.WriteLine($"Warning: Total spending of {tongChiTieu} VND has exceeded the budget limit of {hanMuc.HanMuc} VND.");
                }
                // Thông báo nếu đạt từ 80% đến 100% hạn mức
                else if (tiLe >= 80)
                {
                    Console.WriteLine($"Notice: Total spending of {tongChiTieu} VND has reached {tiLe:F2}% of the budget limit {hanMuc.HanMuc} VND.");
                }
                // Thông báo nếu đạt từ 50% đến 80% hạn mức
                else if (tiLe >= 50)
                {
                    Console.WriteLine($"Notice: Total spending of {tongChiTieu} VND has reached {tiLe:F2}% of the budget limit {hanMuc.HanMuc} VND.");
                }
                // Thông báo nếu chi tiêu dưới 50% hạn mức
                else
                {
                    Console.WriteLine($"Current total spending of {tongChiTieu} VND is within the budget limit of {hanMuc.HanMuc} VND.");
                }
            }
            else
            {
                // Thông báo nếu không tìm thấy hạn mức cho tháng này
                Console.WriteLine("No budget limit found for this month. Please set the budget first.");
            }
        }


        /// <summary>
        /// Function 5: Statistics and Reports
        /// </summary>
        static void ThongKeBaoCao()
        {
            if (danhSachGiaoDich == null || danhSachGiaoDich.Count == 0)
            {
                Console.WriteLine("Transaction list is empty.");
                return;
            }
            // Hiển thị: Chọn phương án thống kê
            Console.WriteLine("Select a reporting option:");
            // Hiển thị: 1. Trong 1 năm
            Console.WriteLine("1. In 1 year");
            // Hiển thị: 2. Trong 6 tháng
            Console.WriteLine("2. In 6 months");
            // Hiển thị: 3. Trong 3 tháng
            Console.WriteLine("3. In 3 months");
            // Hiển thị: 4. Trong 1 tháng
            Console.WriteLine("4. In 1 month");
            // Hiển thị: 5. Trong 1 tuần
            Console.WriteLine("5. In 1 week");
            // Hiển thị: 6. Tùy chỉnh
            Console.WriteLine("6. Custom range");
            // Hiển thị: Nhập lựa chọn của bạn (1-6)
            Console.Write("Enter your choice (1-6): ");

            int luaChon = int.Parse(Console.ReadLine());
            DateTime startDate = DateTime.Now, endDate = DateTime.Now;

            switch (luaChon)
            {
                // Hiển thị: 1. Trong 1 năm
                case 1:
                    startDate = DateTime.Now.AddYears(-1);
                    break;
                // Hiển thị: 2. Trong 6 tháng
                case 2:
                    startDate = DateTime.Now.AddMonths(-6);
                    break;
                // Hiển thị: 3. Trong 3 tháng
                case 3:
                    startDate = DateTime.Now.AddMonths(-3);
                    break;
                // Hiển thị: 4. Trong 1 tháng
                case 4:
                    startDate = DateTime.Now.AddMonths(-1);
                    break;
                // Hiển thị: 5. Trong 1 tuần
                case 5:
                    startDate = DateTime.Now.AddDays(-7);
                    break;
                // Hiển thị: 6. Tùy chỉnh
                case 6:
                    Console.Write("Enter start date (dd/MM/yyyy): ");
                    startDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Console.Write("Enter end date (dd/MM/yyyy): ");
                    endDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    break;
                // Hiển thị: Lựa chọn không hợp lệ
                default:
                    Console.WriteLine("Invalid choice!");
                    return;
            }

            // Hiển thị: Nếu lựa chọn không phải là tùy chỉnh thì kết thúc ngày là hôm nay
            if (luaChon != 6) endDate = DateTime.Now;

            // Thực hiện thống kê: Tính toán khoảng thời gian
            TimeSpan khoangThoiGian = endDate - startDate;
            if (khoangThoiGian.TotalDays > 30)
                ThongKeTheoThang(startDate, endDate);
            else if (khoangThoiGian.TotalDays > 7)
                ThongKeTheoTuan(startDate, endDate);
            else
                ThongKeTheoNgay(startDate, endDate);
        }

        // Tính toán thống kê theo tháng
        static void ThongKeTheoThang(DateTime startDate, DateTime endDate)
        {
            // Lọc giao dịch trong khoảng thời gian bắt đầu và kết thúc
            var giaoDichTrongKhoang = danhSachGiaoDich
                .Where(gd => gd.ThoiGian >= startDate && gd.ThoiGian <= endDate)
                .ToList();
            if (!giaoDichTrongKhoang.Any())
            {
                Console.WriteLine("No transactions found in the selected period.");
                return;
            }

            // Nhóm giao dịch theo tháng và tính tổng chi tiêu theo từng danh mục
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

            // Hiển thị tiêu đề thống kê chi tiêu theo tháng
            Console.WriteLine("\nMonthly Expense Report:");

            // Duyệt qua từng tháng và hiển thị tổng chi tiêu
            foreach (var item in chiTieuTheoThang)
            {
                Console.WriteLine($"Month {item.Thang:MM/yyyy}: {item.TongChiTieu} VND");

                // Hiển thị chi tiết theo từng danh mục
                foreach (var dm in item.TheoDanhMuc)
                {
                    decimal phanTram = ((decimal)dm.TongChiTieu / (decimal)item.TongChiTieu) * 100;
                    Console.WriteLine($"   - {dm.DanhMuc}: {dm.TongChiTieu} VND ({phanTram:F2}%)");
                }
            }
        }

        // Tính toán thống kê theo tuần
        static void ThongKeTheoTuan(DateTime startDate, DateTime endDate)
        {
            // Lọc giao dịch trong khoảng thời gian bắt đầu và kết thúc
            var giaoDichTrongKhoang = danhSachGiaoDich
                .Where(gd => gd.ThoiGian >= startDate && gd.ThoiGian <= endDate)
                .ToList();
            if (!giaoDichTrongKhoang.Any())
            {
                Console.WriteLine("No transactions found in the selected period.");
                return;
            }

            // Nhóm giao dịch theo tuần và tính tổng chi tiêu
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
                            TongChiTieu = subGroup.Sum(gd => gd.SoTien)
                        }).ToList()
                }).ToList();

            // Hiển thị tiêu đề thống kê chi tiêu theo tuần
            Console.WriteLine("\nWeekly Expense Report:");

            // Duyệt qua từng tuần và hiển thị tổng chi tiêu
            foreach (var item in chiTieuTheoTuan)
            {
                Console.WriteLine($"Week {item.Tuan}: {item.TongChiTieu} VND");

                // Hiển thị chi tiết chi tiêu
                foreach (var dm in item.TheoDanhMuc)
                {
                    decimal phanTram = ((decimal)dm.TongChiTieu / (decimal)item.TongChiTieu) * 100;
                    Console.WriteLine($"   - {dm.TongChiTieu} VND ({phanTram:F2}%)");
                }
            }
        }

        // Tính toán thống kê theo ngày
        static void ThongKeTheoNgay(DateTime startDate, DateTime endDate)
        {
            // Lọc giao dịch trong khoảng thời gian bắt đầu và kết thúc
            var giaoDichTrongKhoang = danhSachGiaoDich
                .Where(gd => gd.ThoiGian >= startDate && gd.ThoiGian <= endDate)
                .ToList();
            if (!giaoDichTrongKhoang.Any())
            {
                Console.WriteLine("No transactions found in the selected period.");
                return;
            }

            // Nhóm giao dịch theo ngày và tính tổng chi tiêu cho từng ngày
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

            // Hiển thị báo cáo chi tiêu theo ngày
            Console.WriteLine("\nDaily Expense Report:");

            // Lặp qua từng ngày và hiển thị tổng chi tiêu
            foreach (var item in chiTieuTheoNgay)
            {
                Console.WriteLine($"Date {item.Ngay:dd/MM/yyyy}: {item.TongChiTieu} VND");

                // Hiển thị chi tiết chi tiêu theo danh mục
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
            // Hiển thị tiêu đề Xuất/Nhập Tệp Dữ Liệu
            Console.WriteLine("\n--- Data Import/Export ---");

            // Hiển thị lựa chọn nhập tệp dữ liệu
            Console.WriteLine("1. Import data file");

            // Hiển thị lựa chọn xuất tệp dữ liệu
            Console.WriteLine("2. Export data file");

            // Yêu cầu người dùng chọn chức năng
            Console.Write("Select a function: ");
            string luaChon = Console.ReadLine();

            // Kiểm tra lựa chọn của người dùng
            if (luaChon == "1")
            {
                // Nếu chọn nhập tệp dữ liệu, gọi hàm NhapTepDuLieu
                NhapTepDuLieu();
            }
            else if (luaChon == "2")
            {
                // Nếu chọn xuất tệp dữ liệu, gọi hàm XuatTepDuLieu
                XuatTepDuLieu();
            }
            else
            {
                // Nếu lựa chọn không hợp lệ
                Console.WriteLine("Invalid selection.");
            }
        }


        // Hàm nhập tệp dữ liệu
        static void NhapTepDuLieu()
        {
            // Chọn cách nhập tệp
            Console.WriteLine("\nChoose the file import method:");
            // Nhập tệp mới (Thay thế tệp cũ)
            Console.WriteLine("1. Import new file (Replace old file)");
            // Nhập tệp mới (Kết hợp với tệp cũ)
            Console.WriteLine("2. Import new file (Merge with old file)");

            // Chọn chức năng
            Console.Write("Choose an option: ");
            string luaChon = Console.ReadLine();

            // Lựa chọn không hợp lệ
            if (luaChon != "1" && luaChon != "2")
            {
                // Lỗi khi lựa chọn không hợp lệ
                Console.WriteLine("Invalid choice!");
                return;
            }

            // Nhập đường dẫn tệp cần nhập
            Console.Write("Enter the file path to import: ");
            string duongDanTep = Console.ReadLine();


            // Kiểm tra xem tệp có tồn tại không
            if (!File.Exists(duongDanTep))
            {
                Console.WriteLine("The file does not exist.");
                return;
            }

            // Lấy phần mở rộng của tệp và chuyển về chữ thường
            string fileExtension = Path.GetExtension(duongDanTep).ToLower();

            // Kiểm tra xem tệp có phải là .json hay không
            if (fileExtension != ".json")
            {
                Console.WriteLine("Only .json files are accepted!");
                return;
            }

            try
            {
                // Kiểm tra lựa chọn của người dùng
                if (luaChon == "1")
                {
                    try
                    {
                        // Đọc dữ liệu từ tệp B (đường dẫn do người dùng nhập)
                        string jsonData = File.ReadAllText(duongDanTep);
                        var danhSachGiaoDichMoi = JsonConvert.DeserializeObject<List<TaiChinh.GiaoDich>>(jsonData);

                        // Kiểm tra xem dữ liệu có hợp lệ không
                        if (danhSachGiaoDichMoi == null)
                        {
                            Console.WriteLine("The data in the new file is invalid.");
                            return;
                        }

                        // Yêu cầu người dùng nhập đường dẫn tệp cần thay thế
                        Console.Write("Enter the path of the file to be replaced (old file): ");
                        string duongDanTepCu = Console.ReadLine();

                        // Kiểm tra xem tệp cũ có tồn tại không
                        if (!File.Exists(duongDanTepCu))
                        {
                            Console.WriteLine("The old file does not exist, cannot replace it.");
                            return;
                        }

                        // Lưu dữ liệu từ tệp B vào tệp A (tệp cũ)
                        File.WriteAllText(duongDanTepCu, jsonData);

                        // Cập nhật danh sách giao dịch trong chương trình
                        danhSachGiaoDich = danhSachGiaoDichMoi;

                        // Thông báo thành công
                        Console.WriteLine("The data has been successfully replaced.");
                    }
                    catch (Exception ex)
                    {
                        // Thông báo lỗi nếu có sự cố khi nhập tệp
                        Console.WriteLine($"Error when importing the data file: {ex.Message}");
                    }
                }
                else if (luaChon == "2")
                {
                    // Kết hợp với dữ liệu cũ
                    if (fileExtension == ".json")
                    {
                        string jsonData = File.ReadAllText(duongDanTep);
                        var danhSachGiaoDichMoi = JsonConvert.DeserializeObject<List<TaiChinh.GiaoDich>>(jsonData);
                        danhSachGiaoDich.AddRange(danhSachGiaoDichMoi);
                        Console.WriteLine("The data has been successfully merged");
                    }
                }

                // Lưu lại dữ liệu sau khi nhập
                LuuDuLieu();
            }
            catch (Exception ex)
            {
                // Thông báo lỗi nếu có sự cố khi nhập tệp
                Console.WriteLine($"Error when importing the data file: {ex.Message}");
            }
        }

        // Hàm xuất tệp dữ liệu
        static void XuatTepDuLieu()
        {
            // Yêu cầu người dùng chọn định dạng xuất tệp
            Console.WriteLine("\nSelect the file export format:");

            // Tùy chọn xuất dưới định dạng JSON
            Console.WriteLine("1. Export as JSON format");

            // Tùy chọn xuất dưới định dạng CSV
            Console.WriteLine("2. Export as CSV format");

            // Tùy chọn xuất dưới định dạng TXT
            Console.WriteLine("3. Export as TXT format");

            // Yêu cầu người dùng chọn chức năng
            Console.Write("Choose a function: ");
            string luaChon = Console.ReadLine();

            string duongDanTepXuat = string.Empty;
            string extension = string.Empty;

            // Xử lý lựa chọn của người dùng
            switch (luaChon)
            {
                case "1":
                    // Nếu chọn xuất JSON
                    duongDanTepXuat = "dulieu_giaodich.json";
                    extension = ".json";
                    break;
                case "2":
                    // Nếu chọn xuất CSV
                    duongDanTepXuat = "dulieu_giaodich.csv";
                    extension = ".csv";
                    break;
                case "3":
                    // Nếu chọn xuất TXT
                    duongDanTepXuat = "dulieu_giaodich.txt";
                    extension = ".txt";
                    break;
                default:
                    // Nếu lựa chọn không hợp lệ
                    Console.WriteLine("Invalid choice.");
                    return;
            }

            try
            {
                // Xuất theo định dạng JSON
                if (extension == ".json")
                {
                    // Chuyển danh sách giao dịch thành chuỗi JSON có định dạng đẹp (indentation)
                    string jsonData = JsonConvert.SerializeObject(danhSachGiaoDich, Newtonsoft.Json.Formatting.Indented);
                    // Lưu dữ liệu vào tệp JSON
                    File.WriteAllText(duongDanTepXuat, jsonData);
                }
                // Xuất theo định dạng CSV
                else if (extension == ".csv")
                {
                    // Tạo StringBuilder để xây dựng nội dung CSV
                    var sb = new StringBuilder();
                    // Thêm tiêu đề cột bằng tiếng Anh
                    sb.AppendLine("Serial Number, Description, Amount, Currency Unit, Category, Time");
                    // Lặp qua từng giao dịch và thêm vào chuỗi CSV
                    foreach (var giaoDich in danhSachGiaoDich)
                    {
                        sb.AppendLine($"{giaoDich.SoThuTu},{giaoDich.MoTa},{giaoDich.SoTien},{giaoDich.DonViTienTe},{giaoDich.DanhMuc},{giaoDich.ThoiGian}");
                    }
                    // Lưu dữ liệu vào tệp CSV
                    File.WriteAllText(duongDanTepXuat, sb.ToString());
                }
                // Xuất theo định dạng TXT
                else if (extension == ".txt")
                {
                    // Tạo StringBuilder để xây dựng nội dung TXT
                    var sb = new StringBuilder();
                    // Lặp qua từng giao dịch và thêm thông tin vào chuỗi TXT
                    foreach (var giaoDich in danhSachGiaoDich)
                    {
                        sb.AppendLine($"Serial Number: {giaoDich.SoThuTu}");
                        sb.AppendLine($"Description: {giaoDich.MoTa}");
                        sb.AppendLine($"Amount: {giaoDich.SoTien}");
                        sb.AppendLine($"Currency Unit: {giaoDich.DonViTienTe}");
                        sb.AppendLine($"Category: {giaoDich.DanhMuc}");
                        sb.AppendLine($"Time: {giaoDich.ThoiGian}\n");
                    }
                    // Lưu dữ liệu vào tệp TXT
                    File.WriteAllText(duongDanTepXuat, sb.ToString());
                }

                // Thông báo tệp đã được xuất thành công
                Console.WriteLine($"The file has been successfully exported in {extension} format.");
            }
            catch (Exception ex)
            {
                // Thông báo lỗi nếu có sự cố khi xuất tệp
                Console.WriteLine($"Error when exporting the data file: {ex.Message}");
            }
        }
    }
}
