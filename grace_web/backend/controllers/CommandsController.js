const { validationResult } = require("express-validator");
const mongoose = require("mongoose");

const HttpError = require("../models/HttpError");
const Command = require("../models/Command");

async function getAll(req, res, next) {
  let commands = null;
  try {
    commands = await Command.find({});
    // console.log(resumeData);
  } catch (err) {
    console.log(err);
    return next(
      new HttpError(
        "commands ctrler: something's wrong with the database!",
        500
      )
    );
  }

  if (!commands || commands.length === 0) {
    return next(new HttpError("no command found.", 404));
  }
  // console.log("hereeeeeeee");
  // console.log(notes);
  res.status(200).json({
    commands: commands.map((item) => item.toObject({ getters: true })),
  });
}

async function addNew(req, res, next) {
  const result = validationResult(req).formatWith(
    ({ location, msg, param, value, nestedErrors }) => {
      // Build your resulting errors however you want! String, object, whatever - it works!
      return `${param} has ${msg} >>> ${value}`;
    }
  );
  if (!result.isEmpty()) {
    // Response will contain something like
    // { errors: [ "body[password]: must be at least 10 chars long" ] }
    // return res.json({ errors: result.array() });
    let errorMessage = "";
    result.array().forEach((element) => {
      errorMessage += element + "\n";
    });
    return next(new HttpError(errorMessage, 422));
  }
  const { text, forWhom } = req.body;

  const command = new Command({
    text,
    forWhom,
  });

  try {
    const session = await mongoose.startSession();
    session.startTransaction();
    await command.save({ session });
    // user.places.push(place);
    // await work.save({ session });
    await session.commitTransaction();
  } catch (err) {
    console.log(err);
    return next(new HttpError("Adding new command was failed.", 500));
  }

  console.log(text + " command added.");

  res.status(201).json({ command: command });
}

const deleteById = async (req, res, next) => {
  const id = req.params.commandId; //{placeid:"the value"}
  
  let command = null;
  try {
    command = await Command.findById(id);
  } catch (err) {
    console.log(err);
    return next(
      new HttpError(
        "commands deletion findbyId: something's wrong with the database!",
        500
      )
    );
  }

  if (!command) {
    return next(new HttpError("command with specified id not found.", 404));
  }

  // let user = null;
  // try {
  //   user = await User.findById(place.userId);
  // } catch (error) {
  //   console.log(error);
  //   return next(
  //     new HttpError("users: something's wrong with the database!", 500)
  //   );
  // }

  try {
    const session = await mongoose.startSession();
    session.startTransaction();
    await command.remove({ session });
    // // user.places = user.places.filter((item) => item !== place.placeId);
    // command.commandId.places.pull(command);
    // await command.userId.save({ session });
    await session.commitTransaction();
    
    console.log(id + " command deleted.");
  } catch (err) {
    console.log(err);
    return next(
      new HttpError(
        "command deleteion: something's wrong with the database!",
        402
      )
    );
  }

  res.status(200).json({ msg: "command deleted." });
};

exports.getAll = getAll;
exports.addNew = addNew;
exports.deleteById = deleteById;
