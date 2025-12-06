namespace Doanchuyennganh.Models
{
    public class Paginate
    {
        public int TotalItems { get; set; } // tong so item
        public int PageSize { get; set; } // tong so item/trang
        public int CurrentPage { get; set; } // trang hien tai
        public int TotalPages { get; set; } // tong trang
        public int StartPage { get; set; } // trang bat dau
        public int EndPage { get; set; } // trang ket thuc
        public Paginate()
        {

        }
       public Paginate(int totalItems, int page, int pageSize = 10)
        {
            // lam tron tong items/ 10 items tren 1 trang vd: 16 items/ 10 = tron 3 trang
            int totalPages = (int)Math.Ceiling((decimal)totalItems/(decimal)pageSize);

            int currentPage = page; //page hien tai = 1

            int startPage = currentPage - 5; // trang bat dau tru 5 button
            int endPage = currentPage + 4; // trang cuoi se cong them 4 button

            if(startPage <= 0)
            {
                // neu so trang bat dau nho hon hoac = 0 thi so trang cuoi se bang
                endPage = endPage-(startPage - 1); // 6 - (-3-1) = 10
                startPage = 1;
            }
            if(endPage > totalPages)// neu so trang page cuoi > so tong trang
            {
                endPage = totalPages; // so page cuoi = so tong trang
                if(endPage > 10)// neu so trang cuoi > 10
                {
                    startPage = endPage - 9; // trang bat dau = trang cuoi - 9
                }

            }
            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalItems = totalPages;
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
