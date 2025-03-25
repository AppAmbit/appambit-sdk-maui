using SQLite;

namespace AppAmbit.Models.Analytics;

public class AnalyticsLog
{
    [PrimaryKey]
    public Guid Id { get; set; }
    
    public string EventTitle { get; set; }
    
    public string Data { get; set; }
}