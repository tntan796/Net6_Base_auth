using DNBase.ViewModel;
using FluentValidation;

namespace DNBase.Validators
{
    public class CRUD_MGValidators : AbstractValidator<CRUD_MGRequestModel>
    {
        public CRUD_MGValidators()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Tiêu đề là bắt buộc.").MaximumLength(250).WithMessage("Tiêu đề vượt quá 250 kí tự");
        }
    }
}