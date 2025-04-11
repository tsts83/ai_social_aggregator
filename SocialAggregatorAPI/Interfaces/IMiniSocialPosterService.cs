public interface IMiniSocialPosterService
{
    Task<string> PostNextUnpostedArticleAsync(CancellationToken cancellationToken = default);
}