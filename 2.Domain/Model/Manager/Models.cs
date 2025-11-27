using System.ComponentModel.DataAnnotations;

namespace PT.Domain.Model
{
    public enum CategoryType
    {
        [Display(Name ="Danh mục tin tức")]
        CategoryBlog = 1,

        [Display(Name = "Danh mục dịch vụ")]
        CategoryService = 8,

        [Display(Name = "Tag")]
        Tag = 2,

        [Display(Name = "Tin tức")]
        Blog = 3,

        [Display(Name = "Dịch vụ")]
        Service =4,

        [Display(Name = "Trang nội dung")]
        Page =5,

        [Display(Name = "Câu hỏi thường gặp")]
        FAQ =6,

        [Display(Name = "Nhân viên")]
        Employee =7,

        [Display(Name = "Thư viên ảnh")]
        ImageGallery = 9,

        [Display(Name = "Trang cố định")]
        Static = 10,

        [Display(Name = "Thông tin khuyến mãi")]
        PromotionInformation = 11,

        [Display(Name = "Đăng ký")]
        Register = 12,

        [Display(Name = "Đăng nhập")]
        Signin = 13,

        [Display(Name = "Tính năng")]
        Feature = 14,

        [Display(Name = "Gói cước")]
        Price = 15,

        [Display(Name = "Hợp tác")]
        Cooperate = 16,

        [Display(Name = "Tài liệu hướng dẫn")]
        Document = 17
    }
}
