using Microsoft.AspNetCore.Builder;

namespace Common.EndpointMappers
{
	/// <summary>
	/// Интерфейс описывающий инициализацию конечных точек
	/// </summary>
	public interface IEndpoints
    {
        void MapEndpoints(IApplicationBuilder app);
    }
}
