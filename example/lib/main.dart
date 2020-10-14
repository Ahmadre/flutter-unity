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

class Vehicle {
  static final String model3 = 'model3';
  static final String models = 'models';
  static final String modelx = 'modelx';
  static final String modely = 'modely';
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
  String selectedModel = 'model3';

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
        title: Text(selectedModel?.toUpperCase() ?? ''),
        actions: [
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
          PopupMenuButton<String>(
            onSelected: (String result) {
              setState(() {
                selectedModel = result;
                unityViewController.send(
                  'Loader',
                  'loadScene',
                  selectedModel,
                );
              });
            },
            itemBuilder: (BuildContext context) => <PopupMenuEntry<String>>[
              PopupMenuItem<String>(
                value: Vehicle.model3,
                child: ListTile(
                  leading: Image.asset(
                    'assets/images/model3.png',
                    width: 50,
                  ),
                  title: Text('Model 3'),
                ),
              ),
              PopupMenuItem<String>(
                value: Vehicle.models,
                child: ListTile(
                  leading: Image.asset(
                    'assets/images/models2.png',
                    width: 50,
                  ),
                  title: Text('Model S2'),
                ),
              ),
              /* const PopupMenuItem<Vehicle>(
                value: Vehicle.selfStarter,
                child: Text('Being a self-starter'),
              ),
              const PopupMenuItem<Vehicle>(
                value: Vehicle.tradingCharter,
                child: Text('Placed in charge of trading charter'),
              ), */
            ],
          )
        ],
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
                child: AnimatedSwitcher(
                  duration: const Duration(milliseconds: 250),
                  switchInCurve: Curves.elasticOut,
                  child: Stack(
                    alignment: Alignment.center,
                    children: [
                      AnimatedOpacity(
                        duration: const Duration(milliseconds: 250),
                        curve: Curves.decelerate,
                        opacity: selectedModel != null || (selectedModel?.isNotEmpty ?? false) ? 1.0 : 0.0,
                        child: UnityView(
                          onCreated: onUnityViewCreated,
                          onReattached: onUnityViewReattached,
                          onMessage: onUnityViewMessage,
                        ),
                      ),
                      if (selectedModel == null || (selectedModel?.isEmpty ?? false))
                        Center(
                          child: Column(
                            mainAxisSize: MainAxisSize.min,
                            mainAxisAlignment: MainAxisAlignment.center,
                            crossAxisAlignment: CrossAxisAlignment.center,
                            children: [
                              const Icon(
                                Icons.info_outline,
                                color: Colors.white,
                              ),
                              Text(
                                'Choose a Model',
                                style: TextStyle(color: Colors.white),
                              ),
                            ],
                          ),
                        ),
                    ],
                  ),
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
                          'model',
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
                          'model',
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
                          'model',
                          'DoorOpened',
                          'driverfrontdoor:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Fahrertür schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model',
                          'DoorOpened',
                          'driverfrontdoor:false',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Hintere Fahrertür öffnen'),
                      onPressed: () {
                        unityViewController.send(
                          'model',
                          'DoorOpened',
                          'driverreardoor:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Hintere Fahrertür schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model',
                          'DoorOpened',
                          'driverreardoor:false',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Beifahrertür öffnen'),
                      onPressed: () {
                        unityViewController.send(
                          'model',
                          'DoorOpened',
                          'passengerfrontdoor:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Beifahrertür schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model',
                          'DoorOpened',
                          'passengerfrontdoor:false',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Hintere Beifahrertür öffnen'),
                      onPressed: () {
                        unityViewController.send(
                          'model',
                          'DoorOpened',
                          'passengerreardoor:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Hintere Beifahrertür schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model',
                          'DoorOpened',
                          'passengerreardoor:false',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Trunk öffnen'),
                      onPressed: () {
                        unityViewController.send(
                          'model',
                          'DoorOpened',
                          'trunk:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Trunk schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model',
                          'DoorOpened',
                          'trunk:false',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Frunk öffnen'),
                      onPressed: () {
                        unityViewController.send(
                          'model',
                          'DoorOpened',
                          'frunk:true',
                        );
                      },
                    ),
                    RaisedButton(
                      child: Text('Frunk schließen'),
                      onPressed: () {
                        unityViewController.send(
                          'model',
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
