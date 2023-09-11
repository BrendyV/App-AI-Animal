using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System.Collections.Generic;
using System.IO;

namespace AnimalCUstomVIsion
{
    class Program
    {
        static void Main(string[] args)
        {
            // You can obtain these values from the Keys and Endpoint page for your Custom Vision resource in the Azure Portal.
            string trainingEndpoint = "https://customanimal.cognitiveservices.azure.com/";
            string trainingKey = "a4ebe82590b74f318c5f2d03e6e5b9e1";
            // You can obtain these values from the Keys and Endpoint page for your Custom Vision Prediction resource in the Azure Portal.
            string predictionEndpoint = "https://customanimal.cognitiveservices.azure.com/";
            string predictionKey = "a4ebe82590b74f318c5f2d03e6e5b9e1";
            // You can obtain this value from the Properties page for your Custom Vision Prediction resource in the Azure Portal. See the "Resource ID" field. This typically has a value such as:
            // /subscriptions/<your subscription ID>/resourceGroups/<your resource group>/providers/Microsoft.CognitiveServices/accounts/<your Custom Vision prediction resource name>
            string predictionResourceId = "/subscriptions/c8c6a9a2-21eb-4fb8-98fa-2ddabaeee2ba/resourceGroups/AnimalCustomVIsion/providers/Microsoft.CognitiveServices/accounts/CustomAnimal";

            List<string> hemlockImages;
            List<string> japaneseCherryImages;
            Tag hemlockTag;
            Tag japaneseCherryTag;
            Iteration iteration;
            string publishedModelName = "treeClassModel";
            MemoryStream testImage;

            CustomVisionTrainingClient trainingApi = AuthenticateTraining(trainingEndpoint, trainingKey);
            CustomVisionPredictionClient predictionApi = AuthenticatePrediction(predictionEndpoint, predictionKey);

            Project project = CreateProject(trainingApi);
            AddTags(trainingApi, project);
            UploadImages(trainingApi, project);
            TrainProject(trainingApi, project);
            PublishIteration(trainingApi, project);
            TestIteration(predictionApi, project);
            DeleteProject(trainingApi, project);

        }

        private static CustomVisionTrainingClient AuthenticateTraining(string endpoint, string trainingKey)
        {
            // Create the Api, passing in the training key
            CustomVisionTrainingClient trainingApi = new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(trainingKey))
            {
                Endpoint = endpoint
            };
            return trainingApi;
        }

        private static CustomVisionPredictionClient AuthenticatePrediction(string endpoint, string predictionKey)
        {
            // Create a prediction endpoint, passing in the obtained prediction key
            CustomVisionPredictionClient predictionApi = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = endpoint
            };
            return predictionApi;
        }
    }
}

