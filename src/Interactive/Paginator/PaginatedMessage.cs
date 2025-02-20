﻿namespace Volte.Interactive;

public class PaginatedMessage
{
    public class Builder
    {
        public static Builder New(bool useButtonPaginator = true) => new()
        {
            UseButtonPaginator = useButtonPaginator
        };
        
        public bool UseButtonPaginator { get; private set; }
        
        public IEnumerable<object> Pages { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public IGuildUser Author { get; private set; }
        public Color Color { get; private set; } = new(Config.SuccessColor);
        public string Title { get; private set; }
        public PaginatedAppearanceOptions Options { get; private set; } = PaginatedAppearanceOptions.New;

        public Builder WithPages(IEnumerable<object> pages)
        {
            Pages = pages;
            return this;
        }

        public Builder WithContent(string text)
        {
            Content = text;
            return this;
        }

        public Builder WithAuthor(IGuildUser user)
        {
            Author = user;
            return this;
        }

        public Builder WithColor(Color color)
        {
            Color = color;
            return this;
        }

        public Builder WithTitle(string text)
        {
            Title = text;
            return this;
        }

        public Builder WithOptions(PaginatedAppearanceOptions options)
        {
            Options = options;
            return this;
        }

        public Builder WithDefaults(VolteContext ctx)
        {
            return WithColor(ctx.User.GetHighestRole()?.Color ?? new Color(Config.SuccessColor))
                .WithAuthor(ctx.User);
        }

        public Builder SplitPages(uint perPage) => SplitPages(perPage.Cast<int>());
            
        public Builder SplitPages(int perPage)
        {
            var temp = Pages.ToList();
            var newList = new List<object>();

            do
            {
                newList.Add(temp.Take(perPage).Select(x => x.ToString()).JoinToString("\n"));
                temp.RemoveRange(0, temp.Count < perPage ? temp.Count : perPage);
            } while (temp.Count != 0);

            Pages = newList;
            return this;
        }

        public PaginatedMessage Build()
            => new()
            {
                Pages = Pages as List<object> ?? Pages.ToList(),
                Content = Content,
                Author = Author,
                Color = Color,
                Title = Title,
                Options = Options
            };
            
        private Builder() {}
            
    }
        
    private PaginatedMessage() {}

    /// <summary>
    /// Pages contains a collection of elements to page over in the embed. It is expected
    /// that a string-like object is used in this collection, as objects will be converted
    /// to a displayable string only through their generic ToString method, with the sole
    /// exception of EmbedFieldBuilders.
    /// 
    /// If this collection is of <see cref="EmbedFieldBuilder"/>, then the pages will be displayed in
    /// batches of <see cref="PaginatedAppearanceOptions.FieldsPerPage"/>.
    ///
    /// If this collection is of <see cref="EmbedBuilder"/>, every setting in <see cref="PaginatedMessage"/> will be ignored as
    /// the contents of the EmbedBuilders are used instead.
    /// </summary>
    public List<object> Pages { get; internal set; }

    /// <summary>
    /// Content sets the content of the message, displayed above the embed. This may remain empty.
    /// </summary>
    public string Content { get; internal set; } = string.Empty;

    /// <summary>
    /// Author sets the <see cref="EmbedBuilder.Author"/> property directly.
    /// </summary>
    public IGuildUser Author { get; internal set; }

    public Color Color { get; internal set; } = Color.Default;
    public string Title { get; internal set; }
    public PaginatedAppearanceOptions Options { get; internal set; } = PaginatedAppearanceOptions.New;

    public PaginatedMessage SplitPagesBy(int entriesPerPage)
    {
        var temp = Pages.ToList();
        var newList = new List<object>();

        do
        {
            newList.Add(temp.Take(entriesPerPage).Select(x => x.ToString()).JoinToString("\n"));
            temp.RemoveRange(0, temp.Count < entriesPerPage ? temp.Count : entriesPerPage);
        } while (temp.Any());

        Pages = newList;

        return this;
    }
}