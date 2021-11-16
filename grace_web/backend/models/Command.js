const mongoose = require("mongoose");
const mongooseUniqueValidator = require("mongoose-unique-validator");

const commandSchema = new mongoose.Schema({
    text: {type: String, required: true},
    forWhom: {type: String, default: "{@all@}"}
});

commandSchema.plugin(mongooseUniqueValidator);

module.exports = mongoose.model("Command", commandSchema);

