const SerialPort = require("serialport");

const portName = "COM6"; // Cambia esto al nombre del puerto COM que deseas utilizar

const port = new SerialPort.SerialPort({ path: portName, baudRate: 9600 });

port.on("open", () => {
  console.log(`Puerto serie ${portName} abierto.`);

  /* 2do cliente */
  //const val = ["=005410"];

  /* Obdulio mayorga */
  //const val = ["ST,GS,+      40", "kg"];

  /* Fabian perez */
  //const val = ["ST,GS,+", "160kg", ""];

  /* Hato grande */
  // const val = [
  //   "S",
  //   "T",
  //   ",",
  //   "G",
  //   "S",
  //   ",",
  //   "+",
  //   " ",
  //   " ",
  //   " ",
  //   " ",
  //   " ",
  //   " ",
  //   " ",
  //   "4",
  //   "0",
  //   "k",
  //   "g",
  // ];

  /* grupo hm montecarlos -> 860350196 */
  const val = [
    "U",
    "S",
    "GS",
    ",",
    "+",
    "",
    "",
    "0",
    "kg",
    "",

    "U",
    "S",
    "GS,+",
    "",
    "",
    "1235kg",
    "",
  ];

  let index = 0;
  const intervalId = setInterval(() => {
    if (index < val.length) {
      console.log("values[index]", val[index]);
      port.write(val[index]);
      index++;
    } else {
      index = 0;
    }
  }, 5);
});

port.on("error", (err) => {
  console.error("Error en el puerto serie:", err.message);
});
