using System;
using System.Device.Gpio;
using System.Threading;

public class LightActions
{
    private GpioController controller;
    private readonly CancellationToken cancellationToken;
    private readonly int redPin;
    private readonly int greenPin;
    private readonly int bluePin;

    public LightActions(GpioController controller, CancellationToken cancellationToken, int redPin, int greenPin, int bluePin)
    {
        this.controller = controller;
        this.cancellationToken = cancellationToken;
        this.redPin = redPin;
        this.greenPin = greenPin;
        this.bluePin = bluePin;
    }

    public Action RedLightBlinkerAction
    {
        get
        {
            return () =>
                        {
                            controller.Write(greenPin, PinValue.High);
                            controller.Write(bluePin, PinValue.High);

                            while (true)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                }
                                
                                controller.Write(redPin, PinValue.Low);
                                Thread.Sleep(500);
                                controller.Write(redPin, PinValue.High);
                                Thread.Sleep(500);
                            }
                        };
        }
    }

    public Action GreenLightOnAction
    {
        get
        {
            return () =>
            {
                controller.Write(greenPin, PinValue.Low);
                controller.Write(bluePin, PinValue.High);
                controller.Write(redPin, PinValue.High);
            };
        }
    }

    public Action YellowLightOnAction
    {
        get
        {
            return () =>
            {
                controller.Write(greenPin, PinValue.Low);
                controller.Write(bluePin, PinValue.High);
                controller.Write(redPin, PinValue.Low);
            };
        }
    }

    public Action LightsOffAction
    {
        get
        {
            return () =>
            {
                controller.Write(greenPin, PinValue.High);
                controller.Write(bluePin, PinValue.High);
                controller.Write(redPin, PinValue.High);
            };
        }
    }
}