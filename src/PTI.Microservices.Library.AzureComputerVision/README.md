# PTI.Microservices.Library.AzureComputerVision

Facilitates the consumption of the APIs in Azure Computer Vision

**Examples:**

**Note: The examples below are passing null for the logger, if you want to use the logger make sure to pass the parameter with a value other than null**

## Describe Image
    CustomHttpClient customHttpClient = new CustomHttpClient(new CustomHttpClientHandler(null));
    AzureComputerVisionService azureComputerVisionService =
       new AzureComputerVisionService(null, this.AzureComputerVisionConfiguration,
       customHttpClient);
    Stream imageStream = (await new HttpClient().GetStreamAsync(this.TestFaceImageUrl));
    var result = await azureComputerVisionService.DescribeImageAsync(imageStream,
    Path.GetFileName(this.TestComputerVisionImageUri));

## Analyze Image
    CustomHttpClient customHttpClient = new CustomHttpClient(new CustomHttpClientHandler(null));
    AzureComputerVisionService azureComputerVisionService =
       new AzureComputerVisionService(null, this.AzureComputerVisionConfiguration,
       customHttpClient);
    var allVisualFeatures = Enum.GetValues(typeof(VisualFeature)).Cast<VisualFeature>().ToList();
    var allDetails = Enum.GetValues(typeof(Details)).Cast<Details>().ToList();
    var result = await azureComputerVisionService.AnalyzeImageAsync(new Uri(this.TestComputerVisionImageUri),
    allVisualFeatures, allDetails);

## Read (For Text-heavy images)
    CustomHttpClient customHttpClient = new CustomHttpClient(new CustomHttpClientHandler(null));
    AzureComputerVisionService azureComputerVisionService =
       new AzureComputerVisionService(null, this.AzureComputerVisionConfiguration,
       customHttpClient);
    var result = 
    await azureComputerVisionService.ReadAsync(
       new Uri(ImageWithTextUrl));
    var simpleRebuiltText =
       String.Join(" ", result.readResults.SelectMany(p => p.lines).Select(p => p.text));

## Detect Text (OCR)
    CustomHttpClient customHttpClient = new CustomHttpClient(new CustomHttpClientHandler(null));
    AzureComputerVisionService azureComputerVisionService =
       new AzureComputerVisionService(null, this.AzureComputerVisionConfiguration,
       customHttpClient);
    var result = 
    await azureComputerVisionService.DetectTextAsync(
       new Uri(ImageWithTextUrl), 
       language: DetectImageLanguage.Spanish);