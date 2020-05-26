using System;
using System.Device.Gpio;
using System.Threading;

public class LightActions {
  private GpioController controller;
  private readonly int redPin;
  private readonly int greenPin;
  private readonly int bluePin;

  public LightActions (GpioController controller, int redPin, int greenPin, int bluePin) {
    this.controller = controller;
    this.redPin = redPin;
    this.greenPin = greenPin;
    this.bluePin = bluePin;
  }

  public Action<CancellationToken> RedLightBlinkerAction {
    get {
      return (CancellationToken cancellationToken) => {
        controller.Write (greenPin, PinValue.High);
        controller.Write (bluePin, PinValue.High);

        while (true) {
          if (cancellationToken.IsCancellationRequested) {
            cancellationToken.ThrowIfCancellationRequested ();
          }

          controller.Write (redPin, PinValue.Low);
          Thread.Sleep (500);
          controller.Write (redPin, PinValue.High);
          Thread.Sleep (500);
        }
      };
    }
  }

  public Action<CancellationToken> GreenLightOnAction {
    get {
      return (CancellationToken cancellationToken) => {
        controller.Write (greenPin, PinValue.Low);
        controller.Write (bluePin, PinValue.High);
        controller.Write (redPin, PinValue.High);
      };
    }
  }

  public Action<CancellationToken> YellowLightOnAction {
    get {
      return (CancellationToken cancellationToken) => {
        controller.Write (greenPin, PinValue.Low);
        controller.Write (bluePin, PinValue.High);
        controller.Write (redPin, PinValue.Low);
      };
    }
  }

  public Action<CancellationToken> BlueLightOnAction {
    get {
      return (CancellationToken cancellationToken) => {
        controller.Write (greenPin, PinValue.High);
        controller.Write (bluePin, PinValue.Low);
        controller.Write (redPin, PinValue.High);
      };
    }
  }

  public Action<CancellationToken> LightsOffAction {
    get {
      return (CancellationToken cancellationToken) => {
        controller.Write (greenPin, PinValue.High);
        controller.Write (bluePin, PinValue.High);
        controller.Write (redPin, PinValue.High);
      };
    }
  }
}