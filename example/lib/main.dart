import 'package:flutter/material.dart';
import 'package:flutter_unity/flutter_unity.dart';

void main() => runApp(App());

class App extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      home: HomePage(),
    );
  }
}

class HomePage extends StatefulWidget {
  @override
  _HomePageState createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('3D Model Prototype'),
      ),
      body: Center(
        child: RaisedButton(
          onPressed: () {
            Navigator.push(context, MaterialPageRoute(builder: (context) => UnityViewPage()));
          },
          child: Text('Start Prototype'),
        ),
      ),
    );
  }
}

class UnityViewPage extends StatefulWidget {
  @override
  _UnityViewPageState createState() => _UnityViewPageState();
}

class _UnityViewPageState extends State<UnityViewPage> {
  UnityViewController unityViewController;
  double _sliderValue = 0.0;
  double _speed = 0.0;
  bool isDarkTheme = true;

  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
      appBar: AppBar(
        title: const Text('Model 3'),
        /* actions: [
          IconButton(
            icon: const Icon(Icons.color_lens_outlined),
            onPressed: () {
              if (isDarkTheme) {
                unityViewController.send(
                  'Main Camera',
                  'SetBackgroundColor',
                  '245.0,245.0,245.0',
                );
                setState(() {
                  isDarkTheme = false;
                });
              } else {
                unityViewController.send(
                  'Main Camera',
                  'SetBackgroundColor',
                  '33.0,33.0,33.0',
                );
                setState(() {
                  isDarkTheme = true;
                });
              }
            },
          ),
        ], */
      ),
      body: Card(
        color: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
        ),
        elevation: 10,
        child: SingleChildScrollView(
          physics: BouncingScrollPhysics(),
          child: Column(
            children: [
              AspectRatio(
                aspectRatio: 1.66,
                child: UnityView(
                  onCreated: onUnityViewCreated,
                  onReattached: onUnityViewReattached,
                  onMessage: onUnityViewMessage,
                ),
              ),
              Card(
                child: Column(
                  children: [
                    const Text(
                      'Rotation Speed:',
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(
                      height: 10,
                    ),
                    Slider.adaptive(
                      min: -10.0,
                      max: 10.0,
                      divisions: 20,
                      label: _sliderValue.toInt().toString(),
                      value: _sliderValue,
                      onChanged: (val) {
                        setState(() {
                          _sliderValue = val;
                        });
                        unityViewController.send(
                          'model3',
                          'SetRotationSpeedY',
                          '$val',
                        );
                      },
                    ),
                  ],
                ),
              ),
              Card(
                child: Column(
                  children: [
                    const Text(
                      'Car Speed:',
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(
                      height: 10,
                    ),
                    Slider.adaptive(
                      value: _speed,
                      min: 0,
                      max: 10,
                      divisions: 10,
                      label: _speed.toString(),
                      onChanged: (val) {
                        setState(() {
                          _speed = val;
                        });
                        unityViewController.send(
                          'model3',
                          'SetCarSpeed',
                          '${val.toInt()}',
                        );
                      },
                    ),
                  ],
                ),
              ),
              Card(
                color: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
                child: Wrap(
                  children: [
                    RaisedButton(
                      child: Text('Fahrertür öffnen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'driverfrontdoor:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Fahrertür schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'driverfrontdoor:false',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Hintere Fahrertür öffnen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'driverreardoor:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Hintere Fahrertür schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'driverreardoor:false',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Beifahrertür öffnen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'passengerfrontdoor:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Beifahrertür schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'passengerfrontdoor:false',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Hintere Beifahrertür öffnen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'passengerreardoor:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Hintere Beifahrertür schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'passengerreardoor:false',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Trunk öffnen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'trunk:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Trunk schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'trunk:false',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Frunk öffnen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'frunk:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Frunk schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model3',
                          'DoorOpened',
                          'frunk:false',
                        );
                      },
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void onUnityViewCreated(UnityViewController controller) {
    print('onUnityViewCreated');

    setState(() {
      unityViewController = controller;
    });
  }

  void onUnityViewReattached(UnityViewController controller) {
    print('onUnityViewReattached');
    setState(() {
      unityViewController = controller;
    });
  }

  void onUnityViewMessage(UnityViewController controller, String message) {
    print('onUnityViewMessage');

    print(message);
  }
}
