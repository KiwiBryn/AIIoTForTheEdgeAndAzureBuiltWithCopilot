It all started with a throwaway “how much of a non-trivial AI application do you recon could be built with Copilot”

My “non Trivial” application is monitoring PPE compliance.

On site there would be an industrial computer connected into turn-styles with RFID Tag readers (RS485) and a number of cameras looking at choke points.

Something like a Seeedstudio J3011 Industrial, which is fanless,  has extended operating temperature range, and a TPM.

It has to run disconnected but also upload images and data to Azure for post processing.

For this nasty demo there is a standard Ultralytics Yolo model running on the device looking for humans.

Then once a human is detected the image(s) are uploaded to Azure for processing with a Yolo model designed to identify PPE

This reduces “temporal coupling”. 

Continuous stream of images 10FPS, Yolo Model running on Jetson using CUDA/TensorRT detects humans then stores images with some metadata on the disk.

The uploader pplication then POSTs images and meta data to an Azure WebAPI Service and uses the “Claim Check Pattern”.

Image is put in blob storage, message with blob details  is put in Azure Storage queue – the “inferencing” queue

This reduces “temporal coupling”

The dedicated PPE Detection model is run on a cluster of image processors processor which can automagically scale in response to changes in load.

The images processors pull messages from the queue using the “hungry consumer” pattern
