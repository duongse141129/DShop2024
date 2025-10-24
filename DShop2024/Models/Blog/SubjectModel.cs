using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models.Blog
{
    [Table("Subject")]
    public class SubjectModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} must between {1} to {2} characters")]
        public string Title { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Content")]
        public string SubjectContent { set; get; }


        [StringLength(100, MinimumLength = 5, ErrorMessage = "{0} must between {1} to {2} characters")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Use only characters [a-z0-9-]")]
        public string Slug { set; get; }


        // Subject parent (FKey)
        [Display(Name = "Parent Subject")]
        public int? ParentSubjectId { get; set; }

        //  Subject children
        public ICollection<SubjectModel> SubjectChildren { get; set; }

        [ForeignKey("ParentSubjectId")]
        [Display(Name = "Parent Subject")]
        public SubjectModel ParentSubject { set; get; }

        public int Status { get; set; }

        public void ChildSubjectIDs(ICollection<SubjectModel> childcates, List<int> lists)
        {
            if (childcates == null)
                childcates = this.SubjectChildren;

            foreach (SubjectModel subject in childcates)
            {
                if(subject.Status != 0)
                {
                    lists.Add(subject.Id);
                    ChildSubjectIDs(subject.SubjectChildren, lists);
                }
            }
        }

        public List<SubjectModel> listParents()
        {
            List<SubjectModel> li = new List<SubjectModel>();
            var parent = this.ParentSubject;

            while (parent != null)
            {
                if (parent.Status != 0)
                {
                    li.Add(parent);
                    parent = parent.ParentSubject;
                }
            }
            li.Reverse();
            return li;
        }

    }
}
