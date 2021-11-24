const { validationResult } = require("express-validator");
const mongoose = require("mongoose");

const HttpError = require("../models/HttpError");
const Log = require("../models/Log");

async function getAll(req, res, next) {
  let logs = null;
  try {
    logs = await Log.find({});
    // console.log(resumeData);
  } catch (err) {
    console.log(err);
    return next(
      new HttpError(
        "logs ctrler: something's wrong with the database!",
        500
      )
    );
  }

  if (!logs || logs.length === 0) {
    return next(new HttpError("no log found.", 404));
  }
  // console.log("hereeeeeeee");
  // console.log(notes);
  res.status(200).json({
    logs: logs.map((item) => item.toObject({ getters: true })),
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
    const { text, from } = req.body;

    console.log(`log added text:${text}  from:${from}`);
  
    const log = new Log({
      text,
      from,
    });
  
    try {
      const session = await mongoose.startSession();
      session.startTransaction();
      await log.save({ session });
      // user.places.push(place);
      // await work.save({ session });
      await session.commitTransaction();
    } catch (err) {
      console.log(err);
      return next(new HttpError("Adding new log was failed.", 500));
    }
  
    res.status(201).json({ log: log });
  }

exports.getAll = getAll;
exports.addNew = addNew;
