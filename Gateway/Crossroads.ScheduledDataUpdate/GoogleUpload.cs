using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;

namespace Crossroads.ScheduledDataUpdate
{
    class GoogleUpload
    {
        public void Upload()
        {
            try
            {
                var client = StorageClient.Create();

                // Create a bucket with a globally unique name
                var bucketName = "crds-map-int.appspot.com";
                var bucket = client.GetBucket(bucketName);

                /////////////////////////
                //string fileName = @"C:\Users\Markku\Pictures\mugshot.jpg";
                //System.Drawing.Image i = Image.FromFile("image.jpg");
                //System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
                //var ms = new MemoryStream(sr.Read());

                string filePath = @"C:\Users\Markku\Pictures\mugshot.jpg";
                MemoryStream memStream = new MemoryStream();
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    memStream.SetLength(fileStream.Length);
                    fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);
                }

                // Upload some files
                //var content = Encoding.UTF8.GetBytes("hello, world");
                //var obj1 = client.UploadObject(bucketName, "file1.txt", "text/plain", new MemoryStream(content));
                //var obj2 = client.UploadObject(bucketName, "folder1/file2.txt", "text/plain", new MemoryStream(content));
                var o3 = client.UploadObject(bucketName, "philiscool.jpg", "image/jpeg", memStream);
                Console.WriteLine(o3.MediaLink);
                Console.WriteLine(o3.MediaLink);
                Console.WriteLine(o3.MediaLink);
            }
            catch (Exception ex)
            {
                Console.WriteLine("bob");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
