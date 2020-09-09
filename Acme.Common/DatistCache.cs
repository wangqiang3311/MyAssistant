namespace Acme.Common
{
    public class DatistReport
    {
        public string Type { get; set; }
    }

    public class WeiXinWorkMpNewsArticle : DatistReport
    {
        //     图文消息的标题
        public string Title { get; set; }

        //     图文消息缩略图的media_id, 可以在上传多媒体文件接口中获得。此处thumb_media_id即上传接口返回的media_id
        public byte[] ThumbMedia { get; set; }

        //     图文消息的作者
        public string Author { get; set; }

        //     图文消息点击“阅读原文”之后的页面链接
        public string ContentSourceUrl { get; set; }

        //     图文消息的内容，支持html标签
        public string Content { get; set; }

        //     图文消息的描述
        public string Digest { get; set; }

        //     是否显示封面，1为显示，0为不显示
        public string ShowCoverPic { get; set; }
    }


    public class WeiXinWorkNewsArticle : DatistReport
    {
        //     标题
        public string Title { get; set; }

        //     描述
        public string Description { get; set; }

        //     点击后跳转的链接
        public string Url { get; set; }

        //     图文消息的图片链接，支持JPG、PNG格式，较好的效果为大图640*320，小图80*80。如不填，在客户端不显示图片
        public string PicUrl { get; set; }
    }

    public class File : DatistReport
    {
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
    }

    public class WeiXinWorkText : DatistReport
    {
        public string TextContent { get; set; }
    }

    public class WeiXinWorkFile : DatistReport
    {
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
    }
}
