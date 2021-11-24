const express = require("express");
const controller = require("../controllers/LogsController");
const expressRouter = express.Router();
const { check } = require("express-validator");

expressRouter.get("/", controller.getAll);
expressRouter.post("/", [check("text").notEmpty(), check("from").notEmpty()], controller.addNew);

module.exports = expressRouter;
