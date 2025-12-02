using AppAmbit.Models.Breadcrumbs;

namespace AppAmbit.Models.Breadcrums
{
    public static class BreadcrumbMappings
    {
        public static BreadcrumbsEntity ToEntity(this BreadcrumbData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var id = Guid.TryParse(data.Id, out var guid) ? guid : Guid.NewGuid();
          
            return new BreadcrumbsEntity
            {
                Id = id,
                CreatedAt = data.Timestamp,
                SessionId = data.SessionId ?? "",
                Name = data.Name ?? string.Empty
            };
        }

        public static BreadcrumbData ToData(this BreadcrumbsEntity entity, string? sessionId = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return new BreadcrumbData
            {
                Id = entity.Id.ToString("D"),
                SessionId = sessionId,
                Timestamp = entity.CreatedAt,
                Name = entity.Name
            };
        }

        public static IEnumerable<BreadcrumbsEntity> ToEntities(this IEnumerable<BreadcrumbData> items)
            => items?.Select(d => d.ToEntity()) ?? Enumerable.Empty<BreadcrumbsEntity>();

        public static IEnumerable<BreadcrumbData> ToData(this IEnumerable<BreadcrumbsEntity> items, string? sessionId = null)
            => items?.Select(e => e.ToData(sessionId)) ?? Enumerable.Empty<BreadcrumbData>();
    }
}
