const express = require("express");
const controller = require("../controllers/CommandsController");
const expressRouter = express.Router();
const { check } = require("express-validator");

expressRouter.get("/", controller.getAll);
expressRouter.post("/", [check("text").notEmpty()], controller.addNew);
expressRouter.delete("/:commandId", controller.deleteById);

module.exports = expressRouter;
