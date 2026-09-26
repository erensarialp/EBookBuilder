using System.Collections.Concurrent;
using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace EBookBuilder.Api.Services.Pdf;

public class StartPageCaptureComponent : IDynamicComponent
{
    private readonly int _orderIndex;

    private readonly ConcurrentDictionary<int, int> _startPages;

    public StartPageCaptureComponent(
        int orderIndex,
        ConcurrentDictionary<int, int> startPages)
    {
        _orderIndex = orderIndex;
        _startPages = startPages;
    }

    public DynamicComponentComposeResult Compose(
        DynamicContext context)
    {
        _startPages.AddOrUpdate(
            _orderIndex,
            context.PageNumber,
            (_, existingPage) =>
                Math.Min(
                    existingPage,
                    context.PageNumber
                )
        );

        var content =
            context.CreateElement(
                container =>
                {
                    container
                        .Height(0);
                }
            );

        return new DynamicComponentComposeResult
        {
            Content = content,
            HasMoreContent = false
        };
    }
}