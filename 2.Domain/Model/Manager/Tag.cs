using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public class Tag : IAggregateRoot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Banner { get; set; }
        public string Content { get; set; }
        public bool Status { get; set; }
        [MaxLength(10)]
        public string Language { get; set; }
        public bool Delete { get; set; }
        [NotMapped]
        public virtual Link Link { get; set; }
        [NotMapped]
        public BaseSearchModel<List<ContentPage>> ContentPages { get; set; }
        [NotMapped]
        public List<LinkReference> LinkReferences { get; set; }
    }
    public class TagModel:SeoModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên tag")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 1)]
        public string Name { get; set; }
        public string Banner { get; set; }
        public CategoryType SlugType { get; set; } = CategoryType.Page;
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
    }
    public class AddTagModel 
    {
        public int Id { get; set; }
        [Display(Name = "Tên tag")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 1)]
        public string Name { get; set; }
        public string Language { get; set; }

    }
}
