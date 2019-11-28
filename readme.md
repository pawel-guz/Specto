# Specto (music  visualizer)

Specto is a simple app visualising the music you listen to. Combining it with Arduino-like boards and LED stripes allows creation of original lighting changing to the rhytm of your system sounds. Download and launch the app, connect it with your own, self-made circuit and relax. Specto will try to make this moment a pleasant one!

## How does it work?

Specto listens to your audio device (usiing [CSCore audio library](https://github.com/filoe/cscore) and obtains information about frequencies distribution. Different ranges correspond to different colors. Specto blends them togehter and displays a colorful spectrum. Data is sent to Arduino/other board which takes care of your lighting system.


## Example circuit

Below you can see a complete schematic of the lighting control circuit. Power adapter should provide enough current to supply the led strip. Also, be careful with MOSFET transistors which are static sensitive. They can be damaged by common static charges which build up on your hands or tools. Connect them at the end. Most of the available RGB LED strips require power of 7.2 or 14.4 watts per meter. If you have a 7.2W/m, 5 meter strip, you'll need: 7.2W/m x 5m = 36W of power. The power supply voltage is 12V, so the required current will be: 36W / 12V = 3A. Considering the performance of your AC-DC adapter, a current of about 3.5A should be enough in this example.

[IMAGE]

## Basic board code
Specto can send visualization data to your board via SerialPort or WiFi - direct or network connection (work in progress). See simple receiver code down below. If you want wireless option, see [Specto Receiver](). Using the first option is easier. Just plug in your DIY device, run Specto, enter Device manager tab in Specto and refresh if necessary. That's all, everything should work automatically. But be aware of bugs.

```
// SIMPLE SPECTO RECEIVER by gp
// 255 (NEW) represents the new color transmission or handshake request ahead.
// RGB values are in (0, 254) range.
// Color transmission format: [255][red][green][blue]

static const int SERIAL_SIGNAL = 255;
enum State
{
    AWAIT,
    RED,
    GREEN,
    BLUE,
    END
};

byte red, green, blue;    
byte data, state;

void setup()
{
    Serial.begin(19200);

    pinMode(RED, OUTPUT);
    pinMode(GREEN, OUTPUT);
    pinMode(BLUE, OUTPUT);

    red = 0;
    green = 0; 
    blue = 0;
    state = END;

    setColor(&red, &green, &blue);
}

void loop()
{
    while (Serial.available())
    {
        data = (byte)Serial.read();

        // Process signal.
        if (data == SERIAL_SIGNAL)
        {
            // Really basic handshake. This response is recognized by Specto.
            if (state == AWAIT)
                Serial.write(SERIAL_SIGNAL); 
            else
                state = AWAIT;
        }
        else
        {
            // Process color transmission. State machine.
            switch (state) 
            {
                case AWAIT:
                case RED:
                    red = data;
                    state = GREEN;
                    break;
                case GREEN:
                    green = data;
                    state = BLUE;
                    break;
                case BLUE:
                    blue = data;
                    state = END;
                    setColor(&red, &green, &blue);
                    break;
                default:
                    state = END; 
                    break;
            }
        }
    } 
}

void setColor(const int *r, const int *g, const int *b)
{ 
    analogWrite(RED, r);
    analogWrite(GREEN, g);
    analogWrite(BLUE, b); 
}
```

## License
[MS-PL](https://opensource.org/licenses/MS-PL)
