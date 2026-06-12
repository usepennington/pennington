namespace Pennington.Content;

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Pipeline;

/// <summary>
/// Helpers that collapse the repeated <c>foreach service { … }</c> fan-out
/// patterns across consumers of <see cref="IEnumerable{T}"/> of
/// <see cref="IContentService"/>.
/// </summary>
public static class ContentServiceExtensions
{
    extension(IEnumerable<IContentService> services)
    {
        /// <summary>
        /// Drops every <see cref="IMetaContentService"/> — the services that derive their output from
        /// other services. A meta-service calls this on its sibling set before walking discovery or
        /// records, so meta-services never recurse into each other (or into a transient copy of
        /// themselves whose reference-equality self-check would miss).
        /// </summary>
        public IEnumerable<IContentService> SourceServices()
            => services.Where(s => s is not IMetaContentService);

        /// <summary>Yields every <see cref="DiscoveredItem"/> from every service in registration order.</summary>
        public async IAsyncEnumerable<DiscoveredItem> DiscoverAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var service in services)
            {
                await foreach (var item in service.DiscoverAsync().WithCancellation(cancellationToken))
                {
                    yield return item;
                }
            }
        }

        /// <summary>Yields every <see cref="ContentRecord"/> from every service in registration order.</summary>
        public async IAsyncEnumerable<ContentRecord> GetAllRecordsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var service in services)
            {
                await foreach (var record in service.GetRecordsAsync().WithCancellation(cancellationToken))
                {
                    yield return record;
                }
            }
        }

        /// <summary>Yields every <see cref="ParsedItem"/> from every service (each parsed with its own front-matter type) in registration order.</summary>
        public async IAsyncEnumerable<ParsedItem> ParseAllContentAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var service in services)
            {
                await foreach (var item in service.ParseContentAsync().WithCancellation(cancellationToken))
                {
                    yield return item;
                }
            }
        }

        /// <summary>Concatenates every service's TOC entries into one immutable list.</summary>
        public async Task<ImmutableList<ContentTocItem>> CollectTocEntriesAsync()
        {
            var builder = ImmutableList.CreateBuilder<ContentTocItem>();
            foreach (var service in services)
            {

                builder.AddRange(await service.GetContentTocEntriesAsync());
            }

            return builder.ToImmutable();
        }

        /// <summary>Concatenates every service's indexable entries into one immutable list.</summary>
        public async Task<ImmutableList<ContentTocItem>> CollectIndexableEntriesAsync()
        {
            var builder = ImmutableList.CreateBuilder<ContentTocItem>();
            foreach (var service in services)
            {
                builder.AddRange(await service.GetIndexableEntriesAsync());
            }

            return builder.ToImmutable();
        }

        /// <summary>Concatenates every service's cross-references into one immutable list (no dedup).</summary>
        public async Task<ImmutableList<CrossReference>> CollectCrossReferencesAsync()
        {
            var builder = ImmutableList.CreateBuilder<CrossReference>();
            foreach (var service in services)
            {
                builder.AddRange(await service.GetCrossReferencesAsync());
            }

            return builder.ToImmutable();
        }

        /// <summary>Concatenates every service's static-copy declarations into one immutable list.</summary>
        public async Task<ImmutableList<ContentToCopy>> CollectContentToCopyAsync()
        {
            var builder = ImmutableList.CreateBuilder<ContentToCopy>();
            foreach (var service in services)
            {
                builder.AddRange(await service.GetContentToCopyAsync());
            }

            return builder.ToImmutable();
        }
    }
}