using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Web.Apis;

[Headers("Autherization: Bearer ")]
public interface ICategoryApi
{
    [Post("/api/categories")]
    Task<QuizApiResponse> SaveCategoryAsync(CategoryDto dto);

    [Get("/api/categories")]
    Task<CategoryDto[]> GetCategoriesAsync();
}
