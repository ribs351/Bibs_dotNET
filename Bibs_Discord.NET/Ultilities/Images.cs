using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace Bibs_Discord_dotNET.Utilities
{
    public class Images
    {
        public async Task<string> CreateImageAsync(SocketGuildUser user, string url = null)
        {
            var avatar = await FetchImageAsync(user.GetAvatarUrl(size: 2048, format: Discord.ImageFormat.Png) ?? user.GetDefaultAvatarUrl());
            var background = await FetchImageAsync(url ?? "https://images.unsplash.com/photo-1496715976403-7e36dc43f17b?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=750&q=80");

            background = CropToBanner(background);
            avatar = ClipImageToCircle(avatar);

            var bitmap = avatar as Bitmap;
            bitmap?.MakeTransparent();

            var banner = CopyRegionIntoImage(bitmap, background);
            banner = DrawTextToImage(banner, $"{user.Username}#{user.Discriminator} joined the server", $"Member #{user.Guild.MemberCount}");

            string path = $"{Guid.NewGuid()}.png";
            banner.Save(path);
            return await Task.FromResult(path);
        }

        private static Bitmap CropToBanner(Image image)
        {
            var originalWidth = image.Width;
            var originalHeight = image.Height;
            var destinationSize = new Size(1100, 450);

            var heightRatio = (float)originalHeight / destinationSize.Height;
            var widthRatio = (float)originalWidth / destinationSize.Width;

            var ratio = Math.Min(heightRatio, widthRatio);

            var heightScale = Convert.ToInt32(destinationSize.Height * ratio);
            var widthScale = Convert.ToInt32(destinationSize.Width * ratio);

            var startX = (originalWidth - widthScale) / 2;
            var startY = (originalHeight - heightScale) / 2;

            var sourceRectangle = new Rectangle(startX, startY, widthScale, heightScale);
            var bitmap = new Bitmap(destinationSize.Width, destinationSize.Height);
            var destinationRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            using var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);

            return bitmap;
        }

        private Image ClipImageToCircle(Image image)
        {
            Image destination = new Bitmap(image.Width, image.Height, image.PixelFormat);
            var radius = image.Width / 2;
            var x = image.Width / 2;
            var y = image.Height / 2;

            using Graphics g = Graphics.FromImage(destination);
            var r = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Brush brush = new SolidBrush(Color.Transparent))
            {
                g.FillRectangle(brush, 0, 0, destination.Width, destination.Height);
            }

            var path = new GraphicsPath();
            path.AddEllipse(r);
            g.SetClip(path);
            g.DrawImage(image, 0, 0);
            return destination;
        }

        private Image CopyRegionIntoImage(Image source, Image destination)
        {
            using var grD = Graphics.FromImage(destination);
            var x = (destination.Width / 2) - 110;
            var y = (destination.Height / 2) - 155;

            grD.DrawImage(source, x, y, 220, 220);
            return destination;
        }

        private Image DrawTextToImage(Image image, string header, string subheader)
        {
            //I'm terrible at explaining because I'm not sure what the heck I'm doing most of the time
            //don't need these anymore since image paths asks for font family instead of fonts apparently
            //var roboto = new Font("Roboto", 30, FontStyle.Regular);
            //var robotoSmall = new Font("Roboto", 23, FontStyle.Regular);

            var brushBig = new SolidBrush(Color.White);
            var brushSmall = new SolidBrush(Color.Gray);

            var headerX = image.Width / 2;
            var headerY = (image.Height / 2) + 115;

            var subheaderX = image.Width / 2;
            var subheaderY = (image.Height / 2) + 160;

            var drawFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };
            //make 2 new graphic paths
            GraphicsPath myPathBig = new GraphicsPath();
            GraphicsPath myPathSmall = new GraphicsPath();

            using var GrD = Graphics.FromImage(image);
            //because Graphics.DrawString expects “point size” 
            //while GraphicsPath.AddString expects “em size” 
            //The conversion formula is simply emSize = GrD.DpiY * pointSize / 72.
            //Where GrD.DpiY is the vertical dimension of the image and pointSize being the size you want the text to be
            int emSizeBig = (int)(GrD.DpiY * 30 / 72);
            int emSizeSmall = (int)(GrD.DpiY * 23 / 72);

            //void GraphicsPath.AddString goes something like this
            //(the string you want -> in this case it's the header, font family, font style implicitly casted in int, emSize, origin point, drawFormat )
            myPathBig.AddString(header, new FontFamily("roboto"), (int)FontStyle.Regular, emSizeBig, new Point(headerX, headerY), drawFormat);
            myPathSmall.AddString(subheader, new FontFamily("roboto"), (int)FontStyle.Regular, emSizeSmall, new Point(subheaderX, subheaderY), drawFormat);

            GrD.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit; //forgot what this does, if it's useless in this context then I'll probs get rid of it

            //old code that draws text without the stroke
            //GrD.DrawString(header, roboto, brushWhite, headerX, headerY, drawFormat);
            //GrD.DrawString(subheader, robotoSmall, brushGrey, subheaderX, subheaderY, drawFormat);
            
            GrD.SmoothingMode = SmoothingMode.AntiAlias; //AntiAlias makes stuff looks smoother

            //DrawPath draws the stroke while FillPath fills the text path with color
            GrD.DrawPath(new Pen(Brushes.Black, 4), myPathBig);
            GrD.FillPath(brushBig, myPathBig);
            GrD.DrawPath(new Pen(Brushes.Black, 4), myPathSmall);
            GrD.FillPath(brushSmall, myPathSmall);
            
            var img = new Bitmap(image);
            return img;
        }

        private async Task<Image> FetchImageAsync(string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var backupResponse = await client.GetAsync("https://images.unsplash.com/photo-1496715976403-7e36dc43f17b?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=750&q=80");
                var backupStream = await backupResponse.Content.ReadAsStreamAsync();
                return Image.FromStream(backupStream);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            return Image.FromStream(stream);
        }
    }
}