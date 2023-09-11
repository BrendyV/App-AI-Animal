using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Maui.Graphics.Platform;
using System.Net.Http.Headers;
using System.Windows.Input;
using System.Diagnostics;

namespace AnimalAI.Maui.ViewModels
{
    public class LookingViewModel : BaseViewModel
    {
        string photoPath;
        bool showPhoto;

        private const int ImageMaxSizeBytes = 4194304;
        private const int ImageMaxResolution = 1024;

        public LookingViewModel()
        {
            PickPhotoCommand = new Command(DoPickPhoto);
            CapturePhotoCommand = new Command(DoCapturePhoto, () => MediaPicker.IsCaptureSupported);
        }

        private void LoadPhotoAsync(object obj)
        {
            throw new NotImplementedException();
        }

        public ICommand PickPhotoCommand { get; }
        public ICommand CapturePhotoCommand { get; }

        public bool ShowPhoto
        {
            get => showPhoto;
            set => SetProperty(ref showPhoto, value);
        }

        public string PhotoPath
        {
            get => photoPath;
            set => SetProperty(ref photoPath, value);
        }

        async void DoPickPhoto()
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();

                var resizedPhoto = await ResizePhotoStream(photo);
                var result = await ClassifyImage(new MemoryStream(resizedPhoto));

                await LoadPhotoAsync(photo);

                Console.WriteLine($"PickPhotoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PickPhotoAsync THREW: {ex.Message}");
            }
        }

        async void DoCapturePhoto() //hier kan je de foto nemen
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();

                var resizedPhoto = await ResizePhotoStream(photo);

                var result = await ClassifyImage(new MemoryStream(resizedPhoto));

                await LoadPhotoAsync(photo);

                var percent = result.Probability.ToString("P1");

                Console.WriteLine($"CapturePhotoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
            }
        }



        async Task LoadPhotoAsync(FileResult photo) //Hier word de foto op het scherm getoond
        {
            if (photo == null)
            {
                PhotoPath = null;
                return;
            }

            // save the file into local storage
            var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
            }

            PhotoPath = newFile;
            ShowPhoto = true;



            MakePredictionRequest(PhotoPath);


        }

        public override void OnDisappearing()
        {
            PhotoPath = null;

            base.OnDisappearing();

        }

        public static async Task MakePredictionRequest(string imagePath)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-key", ApiKeys.PredictionKey);

            string url = ApiKeys.CustomVisionEndPoint;

            HttpResponseMessage response;

            byte[] byteData = GetBytesFromImage(imagePath);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);
                Console.WriteLine(await response.Content.ReadAsStringAsync()); //hier doet hij de prediction

            }

        }

        private static byte[] GetBytesFromImage(string imagePath)
        {
            FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(fileStream);
            return binReader.ReadBytes((int)fileStream.Length); //hier leest hij hoeveel bytes de pic heeft
        }

        private async Task<byte[]> ResizePhotoStream(FileResult photo)
        {
            byte[] result = null;

            using (var stream = await photo.OpenReadAsync())
            {
                if (stream.Length > ImageMaxSizeBytes)
                {
                    var image = PlatformImage.FromStream(stream);
                    if (image != null)
                    {
                        var newImage = image.Downsize(ImageMaxResolution, true);
                        result = newImage.AsBytes();
                    }
                }
                else
                {
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        result = binaryReader.ReadBytes((int)stream.Length);
                    }
                }
            }

            return result;
        }

        private async Task<PredictionModel> ClassifyImage(Stream photoStream)
        {
            try
            {

                var endpoint = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(ApiKeys.PredictionKey))
                {
                    Endpoint = ApiKeys.CustomVisionEndPoint
                };

                // Send image to the Custom Vision API
                var results = await endpoint.ClassifyImageAsync(Guid.Parse(ApiKeys.ProjectId), ApiKeys.PublishedName, photoStream);

                // Return the most likely prediction
                return results.Predictions?.OrderByDescending(x => x.Probability).FirstOrDefault();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new PredictionModel();
            }
        }
    }
}
