using AutoMapper;
using BookExchange.Db.Entities;
using BookExchange.Web.ViewModels;

namespace BookExchange.Web.Helpers;

/// <summary>
/// Конфигурация AutoMapper: какие Entity во что маппятся.
/// Регистрируется в Program.cs через <c>services.AddAutoMapper(typeof(MappingProfile))</c>.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Book -> BookCardViewModel
        CreateMap<Book, BookCardViewModel>()
            .ForMember(d => d.ConditionLabel, o => o.MapFrom(s => ConditionToLabel(s.Condition)))
            .ForMember(d => d.Owner, o => o.MapFrom(s => s.Owner));

        // Book -> BookDetailsViewModel
        CreateMap<Book, BookDetailsViewModel>()
            .ForMember(d => d.ConditionLabel, o => o.MapFrom(s => ConditionToLabel(s.Condition)))
            .ForMember(d => d.Owner, o => o.MapFrom(s => s.Owner));

        // User -> OwnerSummaryViewModel
        CreateMap<User, OwnerSummaryViewModel>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.Avatar, o => o.MapFrom(s => s.AvatarPath))
            .ForMember(d => d.BooksCount, o => o.MapFrom(s => s.Books != null ? s.Books.Count : 0))
            .ForMember(d => d.ExchangesCount, o => o.Ignore());

        // BookFormViewModel -> Book (для создания/редактирования)
        CreateMap<BookFormViewModel, Book>()
            .ForMember(d => d.Id, o => o.Condition(src => src.Id.HasValue))
            .ForMember(d => d.CoverImagePath, o => o.Ignore()) // заполняется вручную после загрузки файла
            .ForMember(d => d.OwnerId, o => o.Ignore());

        CreateMap<Book, BookFormViewModel>()
            .ForMember(d => d.ExistingCoverPath, o => o.MapFrom(s => s.CoverImagePath))
            .ForMember(d => d.CoverImage, o => o.Ignore());
    }

    public static string ConditionToLabel(BookCondition c) => c switch
    {
        BookCondition.Excellent => "отличное",
        BookCondition.Good => "хорошее",
        BookCondition.Acceptable => "удовлетворительное",
        _ => c.ToString()
    };

    public static string StatusToLabel(ExchangeStatus s) => s switch
    {
        ExchangeStatus.Pending => "Ожидает ответа",
        ExchangeStatus.Accepted => "Принят",
        ExchangeStatus.Rejected => "Отклонён",
        ExchangeStatus.Completed => "Завершён",
        ExchangeStatus.Cancelled => "Отменён",
        _ => s.ToString()
    };
}
