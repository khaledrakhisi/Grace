const express = require("express");
const mongoose = require("mongoose");
const path = require("path");

const HttpError = require("./models/HttpError");
const CommandsRouter = require("./Routes/CommandsRoute");

const app = express();
const PORT = process.env.PORTNUM;
const DB_USER = process.env.DB_USER;
const DB_PASSWORD = encodeURIComponent(process.env.DB_PASSWORD);
const DB_NAME = process.env.DB_NAME;
const DB_URL = `mongodb+srv://${DB_USER}:${DB_PASSWORD}@cluster0.3h5cz.mongodb.net/${DB_NAME}?retryWrites=true&w=majority`;

// app.use(express.urlencoded()); //Parse URL-encoded bodies
app.use(express.json());

app.use("/api/commands", CommandsRouter);

app.use((req, res, next) => {
  throw new HttpError("Backend is ok! but Page not found. " + req.url, 404);
});

// when we provide four parameters for the 'use' function,
// express interprets it as an Error Handler middleware
app.use((error, req, res, next) => {
  if (res.headerSent) {
    return next(error);
  }
  res
    .status(error.code || 500)
    .json({ msg: error.message || "an error accured" });
});

mongoose
  .connect(DB_URL, {
    useNewUrlParser: true,
    useUnifiedTopology: true,
    useCreateIndex: true,
  })
  .then(() => {
    console.log(`Connecting to the database was successfully!`);
    // if connection to the db was ok then
    app.listen(PORT, (err) => {
      if (err) console.log(err);
      else console.log(`Server started listening to PORT ${PORT}.`);
    });
  })
  .catch((err) => {
    console.log(err);
  });


//   const commands = {
//     commands: [
//       {
//         id: 1,
//         text: "sys bsod",
//       },
//       {
//         id: 2,
//         text: "agent st",
//       },
//     ],
//   };