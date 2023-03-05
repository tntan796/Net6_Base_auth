using DNBase.ViewModel;
using FluentValidation;

namespace DNBase.Validators
{
    public class CRUDValidators : AbstractValidator<CRUDRequestModel>
    {
        public CRUDValidators()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Tiêu đề là bắt buộc.").MaximumLength(250).WithMessage("Tiêu đề vượt quá 250 kí tự");
        }
    }
}