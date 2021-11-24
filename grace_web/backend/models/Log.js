const mongoose = require("mongoose");
const mongooseUniqueValidator = require("mongoose-unique-validator");
const moment = require("moment");

// const todayDate = moment(new Date()).format('YYYY-MM-DD');
// const todayTime = moment(new Date()).format('h:mm:ss a');

const logSchema = new mongoose.Schema({
    text: {type: String, required: true},
    from: {type: String, required: true},
    date: {type: Date, default: moment(new Date()).format('YYYY-MM-DD HH:mm:ss')},
    // time: {type: Date, default: todayTime}
});

logSchema.plugin(mongooseUniqueValidator);

module.exports = mongoose.model("Log", logSchema);

