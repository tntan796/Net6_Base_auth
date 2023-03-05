using DNBase.ViewModel;
using FluentValidation;

namespace DNBase.Validators
{
    public class QT_QuyenValidators : AbstractValidator<QT_QuyenRequestModel>
    {
        public QT_QuyenValidators()
        {
            //RuleFor(x => x.HinhThucGhiNhan).NotEmpty().WithMessage("Hình thức ghi nhận là bắt buộc.").MaximumLength(50).WithMessage("Hình thực ghi nhận vượt quá 50 kí tự");
        }
    }
}