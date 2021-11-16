const mongoose = require("mongoose");
const mongooseUniqueValidator = require("mongoose-unique-validator");

const historySchema = new mongoose.Schema({
    text: {type: String, required: true},  
    byWhom: {type: String, required: true},
    date_picked : {type: Date, required: true, default: Date.now}
});

historySchema.plugin(mongooseUniqueValidator);

module.exports = mongoose.model("History", historySchema);

