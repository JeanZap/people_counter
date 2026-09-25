import cv2

cap = cv2.VideoCapture(
    "rtsp://admin:12345678ab@192.168.0.18:554/onvif1",
    cv2.CAP_FFMPEG
)

# Check if the video was opened successfully
if not cap.isOpened():
    print("Error: Could not open video file.")
    exit()

# Get frame width and height
frame_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
frame_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

# Define the codec and create VideoWriter object
fourcc = cv2.VideoWriter_fourcc(*"XVID")
out = cv2.VideoWriter("output.avi", fourcc, 30.0, (frame_width, frame_height))
while True:
    ret, frame = cap.read()
    if not ret:
        print("End of video or error occurred.")
        break

    # Write the frame to the output video file
    out.write(frame)

    # Display the frame
    cv2.imshow("Frame", frame)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release everything
cap.release()
out.release()
cv2.destroyAllWindows()
